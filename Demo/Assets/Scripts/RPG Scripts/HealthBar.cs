using UnityEngine;

[RequireComponent(typeof(Combatant))]
public class HealthBar : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite fillSprite;

    [Header("Size And Placement")]
    [SerializeField] private float width = 1.2f;
    [SerializeField] private float height = 0.18f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.25f);
    [SerializeField] private bool placeAboveSprite = true;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    [SerializeField] private Color partyFillColor = new Color(0.35f, 0.95f, 0.4f, 1f);
    [SerializeField] private Color enemyFillColor = new Color(0.95f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color delayedColor = new Color(1f, 1f, 1f, 1f);

    [Header("Label")]
    [SerializeField] private bool showLabel = true;
    [SerializeField] private Font labelFont;
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private bool showMaxHealth = true;
    [SerializeField] private float labelSize = 0.055f;
    [SerializeField] private Vector2 labelOffset = new Vector2(0f, 0.16f);

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 6f;
    [SerializeField] private float delayedSpeed = 1.5f;
    [SerializeField] private float delayBeforeCatchUp = 0.4f;

    [Header("Rendering")]
    [SerializeField] private int sortingOrderBoost = 200;
    [SerializeField] private bool hideWhenDead = true;

    private Combatant combatant;
    private Transform root;
    private Transform fillPivot;
    private Transform delayedPivot;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer delayedRenderer;

    private TextMesh label;
    private int lastShownValue = -1;

    private float shown = 1f;
    private float delayed = 1f;
    private float delayTimer;
    private float verticalAnchor;

    void Awake()
    {
        combatant = GetComponent<Combatant>();

        if (backgroundSprite == null || fillSprite == null)
        {
            Debug.LogError("[HealthBar] " + name + " needs Background Sprite and Fill Sprite assigned.", this);
            enabled = false;
            return;
        }

        verticalAnchor = placeAboveSprite ? MeasureTop() : 0f;
        Build();
        SnapToCurrent();
    }

    private float MeasureTop()
    {
        SpriteRenderer[] found = GetComponentsInChildren<SpriteRenderer>(true);
        if (found.Length == 0) {
            return 0.5f;
        }

        bool started = false;
        Bounds total = new Bounds(transform.position, Vector3.zero);

        foreach (SpriteRenderer r in found)
        {
            if (r.sprite == null) continue;

            if (!started)
            {
                total = r.bounds;
                started = true;
            }
            else total.Encapsulate(r.bounds);
        }

        if (!started) return 0.5f;
        return total.max.y - transform.position.y;
    }

    private void Build()
    {
        SpriteRenderer reference = GetComponentInChildren<SpriteRenderer>(true);
        int layerId = reference != null ? reference.sortingLayerID : 0;
        int order = reference != null ? reference.sortingOrder : 0;

        GameObject rootObject = new GameObject("HealthBar_" + name);
        root = rootObject.transform;

        backgroundRenderer = MakeBar(backgroundSprite, backgroundColor, layerId, order + sortingOrderBoost, out Transform ignored, false);
        delayedRenderer = MakeBar(fillSprite, delayedColor, layerId, order + sortingOrderBoost + 1, out delayedPivot, true);
        fillRenderer = MakeBar(fillSprite, combatant.isPartyMember ? partyFillColor : enemyFillColor, layerId, order + sortingOrderBoost + 2, out fillPivot, true);

        if (showLabel) 
        {
            BuildLabel(layerId, order + sortingOrderBoost + 3);
        }
    }

    private void BuildLabel(int layerId, int order)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(root, false);
        labelObject.transform.localPosition = new Vector3(labelOffset.x, labelOffset.y, 0f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = labelColor;
        label.fontSize = 64;
        label.characterSize = labelSize;

        MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();

        Font chosen = labelFont;
        if (chosen == null) chosen = LoadBuiltinFont();

        if (chosen != null)
        {
            label.font = chosen;
            meshRenderer.sharedMaterial = chosen.material;
        }
        else
        {
            Debug.LogWarning("[HealthBar] " + name + " has no font. Assign one in the Label Font field.", this);
        }

        meshRenderer.sortingLayerID = layerId;
        meshRenderer.sortingOrder = order;
    }

    private Font LoadBuiltinFont()
    {
        string[] names = { "LegacyRuntime.ttf", "Arial.ttf" };

        foreach (string fontName in names)
        {
            try
            {
                Font found = Resources.GetBuiltinResource<Font>(fontName);
                if (found != null) return found;
            }
            catch { }
        }

        return null;
    }

    private void RefreshLabel()
    {
        if (label == null) return;

        int value = combatant.CurrentHealth;
        if (value == lastShownValue) return;

        lastShownValue = value;
        label.text = showMaxHealth ? value + "/" + combatant.MaxHealth : value.ToString();
    }

    private SpriteRenderer MakeBar(Sprite sprite, Color color, int layerId, int order, out Transform pivot, bool anchorLeft)
    {
        GameObject pivotObject = new GameObject(anchorLeft ? "Pivot" : "Background");
        pivot = pivotObject.transform;
        pivot.SetParent(root, false);

        GameObject spriteObject = new GameObject("Sprite");
        spriteObject.transform.SetParent(pivot, false);

        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerID = layerId;
        renderer.sortingOrder = order;

        Vector2 size = sprite.bounds.size;
        float scaleX = size.x > 0f ? width / size.x : 1f;
        float scaleY = size.y > 0f ? height / size.y : 1f;
        spriteObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        if (anchorLeft)
        {
            pivot.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
            spriteObject.transform.localPosition = new Vector3(width * 0.5f, 0f, 0f);
        }

        return renderer;
    }

    private void SnapToCurrent()
    {
        float ratio = Ratio();
        shown = ratio;
        delayed = ratio;
        ApplyScales();
        RefreshLabel();
        FollowTarget();
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.gameObject.SetActive(visible);
        }
    }

    private float Ratio()
    {
        if (combatant.MaxHealth <= 0) return 0f;
        return Mathf.Clamp01((float)combatant.CurrentHealth / combatant.MaxHealth);
    }

    void LateUpdate()
    {
        if (root == null) return;

        FollowTarget();

        float ratio = Ratio();

        if (ratio < shown)
        {
            shown = Mathf.MoveTowards(shown, ratio, Time.deltaTime * fillSpeed);
            delayTimer = delayBeforeCatchUp;
        }
        else if (ratio > shown)
        {
            shown = Mathf.MoveTowards(shown, ratio, Time.deltaTime * fillSpeed);
            delayed = Mathf.Max(delayed, shown);
        }

        if (delayed > shown)
        {
            if (delayTimer > 0f) delayTimer -= Time.deltaTime;
            else delayed = Mathf.MoveTowards(delayed, shown, Time.deltaTime * delayedSpeed);
        }
        else delayed = shown;

        ApplyScales();
        RefreshLabel();

        if (hideWhenDead && !combatant.IsAlive && shown <= 0.001f && delayed <= 0.001f)
            root.gameObject.SetActive(false);
    }

    private void FollowTarget()
    {
        root.position = transform.position + new Vector3(offset.x, verticalAnchor + offset.y, 0f);
        root.rotation = Quaternion.identity;
    }

    private void ApplyScales()
    {
        if (fillPivot != null) fillPivot.localScale = new Vector3(Mathf.Max(shown, 0f), 1f, 1f);
        if (delayedPivot != null) delayedPivot.localScale = new Vector3(Mathf.Max(delayed, 0f), 1f, 1f);
    }

    void OnDestroy()
    {
        if (root != null) Destroy(root.gameObject);
    }
}