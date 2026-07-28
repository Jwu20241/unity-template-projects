using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Camera cam;
    [SerializeField] private KeyCode cyclePrevKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode cycleNextKey = KeyCode.RightArrow;
    [SerializeField] private bool logMouseMisses = true;

    private bool selecting;
    private List<Combatant> candidates = new List<Combatant>();
    private Combatant hovered;
    private Combatant selected;
    private Combatant lastSelected;
    private float nextMissLog;

    public bool IsSelecting => selecting;
    public Combatant Selected => selected;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (cam == null)
            Debug.LogError("[TargetSelector] No camera. Tag your camera as MainCamera or assign it.", this);
        else if (!cam.orthographic)
            Debug.LogError("[TargetSelector] Camera is Perspective. Set Projection to Orthographic for 2D.", cam);

        if (targetLayer.value == 0)
            Debug.LogError("[TargetSelector] Target Layer mask is empty. Set it to your Targetable layer.", this);
    }

    public void Begin(List<Combatant> validTargets)
    {
        candidates = new List<Combatant>();

        foreach (Combatant c in validTargets)
        {
            if (c == null || !c.IsAlive) continue;

            Collider2D col = c.GetComponentInChildren<Collider2D>(true);
            if (col == null)
            {
                Debug.LogError("[TargetSelector] " + c.name + " has no Collider2D on itself or any child. It cannot be clicked.", c);
            }
            else
            {
                if (!col.enabled)
                    Debug.LogWarning("[TargetSelector] " + c.name + " has a disabled Collider2D.", c);

                if ((targetLayer.value & (1 << col.gameObject.layer)) == 0)
                    Debug.LogError("[TargetSelector] " + col.name + " (collider for " + c.name + ") is on layer '"
                        + LayerMask.LayerToName(col.gameObject.layer)
                        + "' which is not in Target Layer. It cannot be clicked.", col);
            }

            if (!c.HasOutline)
                Debug.LogWarning("[TargetSelector] " + c.name + " has no outline object, so it will not highlight.", c);

            candidates.Add(c);
        }

        selecting = candidates.Count > 0;
        hovered = null;
        selected = null;

        Debug.Log("[TargetSelector] Begin with " + candidates.Count + " candidates. selecting=" + selecting);

        if (selecting)
        {
            Combatant preferred = candidates[0];

            if (lastSelected != null && lastSelected.IsAlive && candidates.Contains(lastSelected))
                preferred = lastSelected;

            Select(preferred);
        }
    }

    public void Cancel()
    {
        if (hovered != null) hovered.SetHovered(false);
        if (selected != null) selected.SetSelected(false);

        hovered = null;
        selected = null;
        selecting = false;
        candidates.Clear();
    }

    private void Select(Combatant target)
    {
        if (selected == target) return;

        if (selected != null) selected.SetSelected(false);
        selected = target;
        if (selected != null)
        {
            selected.SetSelected(true);
            lastSelected = selected;
        }

        Debug.Log("[TargetSelector] Selected " + (selected != null ? selected.name : "nothing"));
    }

    private void Cycle(int direction)
    {
        if (candidates.Count == 0) return;

        int start = candidates.IndexOf(selected);
        if (start < 0) start = 0;

        for (int step = 1; step <= candidates.Count; step++)
        {
            int i = start + direction * step;
            i = ((i % candidates.Count) + candidates.Count) % candidates.Count;

            if (candidates[i].IsAlive)
            {
                Select(candidates[i]);
                return;
            }
        }
    }

    void Update()
    {
        if (!selecting || cam == null) return;

        Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(world, targetLayer);

        Combatant candidate = null;
        if (hit != null)
        {
            Combatant found = hit.GetComponentInParent<Combatant>();
            if (found != null && found.IsAlive && candidates.Contains(found)) candidate = found;
        }

        if (candidate != hovered)
        {
            if (hovered != null) hovered.SetHovered(false);
            hovered = candidate;
            if (hovered != null)
            {
                hovered.SetHovered(true);
                Debug.Log("[TargetSelector] Hovering " + hovered.name);
            }
        }

        if (logMouseMisses && hit == null && Time.time >= nextMissLog)
        {
            nextMissLog = Time.time + 1f;
            Debug.Log("[TargetSelector] Mouse world position " + world
                + " hit nothing on the target layer. Check collider size, layer, and camera Z.");
        }

        if (hovered != null && Input.GetMouseButtonDown(0)) Select(hovered);

        if (Input.GetKeyDown(cycleNextKey)) Cycle(1);
        if (Input.GetKeyDown(cyclePrevKey)) Cycle(-1);
    }
}