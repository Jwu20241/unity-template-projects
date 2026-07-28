using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Camera cam;
    [SerializeField] private KeyCode cyclePrevKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode cycleNextKey = KeyCode.RightArrow;
    [SerializeField] private float clickFallbackRadius = 2f;
    [SerializeField] private bool useColliders = true;
    [SerializeField] private bool verbose = true;

    private bool selecting;
    private List<Combatant> candidates = new List<Combatant>();
    private Combatant hovered;
    private Combatant selected;
    private Combatant lastSelected;

    public bool IsSelecting => selecting;
    public Combatant Selected => selected;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (cam == null)
            Debug.LogError("[TargetSelector] No camera found. Tag one as MainCamera or assign it.", this);
        else if (!cam.orthographic)
            Debug.LogWarning("[TargetSelector] Camera is Perspective. Orthographic is expected for 2D.", cam);
    }

    public void Begin(List<Combatant> validTargets)
    {
        candidates = new List<Combatant>();

        foreach (Combatant c in validTargets)
        {
            if (c != null && c.IsAlive) candidates.Add(c);
        }

        selecting = candidates.Count > 0;
        hovered = null;
        selected = null;

        if (verbose)
        {
            string list = "";
            for (int i = 0; i < candidates.Count; i++) list += "[" + (i + 1) + "] " + candidates[i].name + "  ";
            Debug.Log("[TargetSelector] Targets: " + list + "Click, arrow keys, or number keys to choose.");
        }

        if (!selecting) return;

        Combatant preferred = candidates[0];
        if (lastSelected != null && lastSelected.IsAlive && candidates.Contains(lastSelected))
            preferred = lastSelected;

        Select(preferred);
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
        if (target == null || !candidates.Contains(target)) return;
        if (selected == target) return;

        if (selected != null) selected.SetSelected(false);

        selected = target;
        selected.SetSelected(true);
        lastSelected = selected;

        if (verbose) Debug.Log("[TargetSelector] Selected " + selected.name);
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

    private Combatant UnderMouse(Vector2 world)
    {
        if (!useColliders) return null;

        Collider2D[] hits = Physics2D.OverlapPointAll(world, targetLayer);

        foreach (Collider2D h in hits)
        {
            Combatant c = h.GetComponentInParent<Combatant>();
            if (c != null && c.IsAlive && candidates.Contains(c)) return c;
        }

        return null;
    }

    private Combatant NearestTo(Vector2 world, float maxDistance)
    {
        Combatant best = null;
        float bestDistance = maxDistance;

        foreach (Combatant c in candidates)
        {
            if (c == null || !c.IsAlive) continue;

            float d = Vector2.Distance(world, c.transform.position);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = c;
            }
        }

        return best;
    }

    void Update()
    {
        if (!selecting || cam == null) return;

        Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);

        Combatant candidate = UnderMouse(world);
        if (candidate == null) candidate = NearestTo(world, clickFallbackRadius);

        if (candidate != hovered)
        {
            if (hovered != null) hovered.SetHovered(false);
            hovered = candidate;
            if (hovered != null) hovered.SetHovered(true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Combatant picked = UnderMouse(world);
            if (picked == null) picked = NearestTo(world, clickFallbackRadius);

            if (picked != null) Select(picked);
            else if (verbose) Debug.Log("[TargetSelector] Click at " + world + " matched no target.");
        }

        for (int i = 0; i < candidates.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Select(candidates[i]);
        }

        if (Input.GetKeyDown(cycleNextKey)) Cycle(1);
        if (Input.GetKeyDown(cyclePrevKey)) Cycle(-1);
    }
}