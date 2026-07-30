using System.Collections;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    [Header("Stats")]
    public int speed = 10;
    public int strength = 10;
    public int mp = 10;
    public int maxHealth = 10;
    public int turnPriority = 0;
    public bool isPartyMember;

    [Header("Highlight Tint")]
    [SerializeField] private Color selectedTint = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color hoverTint = new Color(1f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color activeTurnTint = new Color(0.5f, 0.9f, 1f, 1f);
    [SerializeField] private Color deadTint = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color defendTint = new Color(1f, 0.95f, 0.2f, 1f);

    [Header("Active Turn Motion")]
    [SerializeField] private float activeScale = 1.18f;
    [SerializeField] private float bounceHeight = 0.14f;
    [SerializeField] private float bounceSpeed = 5f;
    [SerializeField] private float easeSpeed = 10f;

    [Header("Attack Lunge")]
    [SerializeField] private float lungeDistance = 0.6f;
    [SerializeField] private float lungeOutTime = 0.10f;
    [SerializeField] private float lungeHoldTime = 0.06f;
    [SerializeField] private float lungeBackTime = 0.16f;

    [Header("Runtime")]
    [SerializeField] private int currentHealth;

    public bool IsAlive => currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasOutline => renderers != null && renderers.Length > 0;

    private SpriteRenderer[] renderers;
    private Color[] baseColors;

    private bool hovered;
    private bool selected;
    private bool activeTurn;
    private bool isDefending;

    private Vector3 baseScale;
    private Vector3 basePosition;
    private Coroutine motionRoutine;

    void Awake()
    {
        currentHealth = maxHealth;
        baseScale = transform.localScale;
        basePosition = transform.localPosition;

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            baseColors[i] = renderers[i].color;
        }

        if (renderers.Length == 0)
            Debug.LogError("[Combatant] " + name + " has no SpriteRenderer on itself or any child.", this);
        else
            Debug.Log("[Combatant] " + name + " tinting " + renderers.Length + " renderers.", this);
    }

    public void SetHovered(bool on)
    {
        hovered = on;
        RefreshTint();
    }

    public void SetSelected(bool on)
    {
        selected = on;
        RefreshTint();
    }

    public void SetActiveTurn(bool on)
    {
        activeTurn = on;
        RefreshTint();

        if (motionRoutine != null) {
            StopCoroutine(motionRoutine);
        } 
        motionRoutine = StartCoroutine(on ? BounceLoop() : ReturnToRest());
    }

    public void SetDefending(bool on)
    {
        isDefending = on;
        RefreshTint();
    }

    public IEnumerator PlayAttack(Combatant target)
    {
        if (motionRoutine != null) StopCoroutine(motionRoutine);

        float dir = 1f;
        if (target != null)
            dir = Mathf.Sign(target.transform.position.x - transform.position.x);
        if (dir == 0f) dir = 1f;

        Vector3 from = basePosition;
        Vector3 to = basePosition + Vector3.right * dir * lungeDistance;

        yield return MoveOver(from, to, lungeOutTime);
        yield return new WaitForSeconds(lungeHoldTime);
        yield return MoveOver(to, from, lungeBackTime);

        transform.localPosition = basePosition;
        motionRoutine = StartCoroutine(activeTurn ? BounceLoop() : ReturnToRest());
    }

    private IEnumerator MoveOver(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            transform.localPosition = Vector3.Lerp(from, to, p * p * (3f - 2f * p));
            yield return null;
        }
        transform.localPosition = to;
    }

    private IEnumerator BounceLoop()
    {
        float t = 0f;
        Vector3 target = baseScale * activeScale;

        while (true)
        {
            t += Time.deltaTime;

            float bob = Mathf.Abs(Mathf.Sin(t * bounceSpeed)) * bounceHeight;
            float squash = 1f - bob * 0.25f;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                new Vector3(target.x, target.y * squash, target.z),
                Time.deltaTime * easeSpeed);

            transform.localPosition = basePosition + Vector3.up * bob;
            yield return null;
        }
    }

    private IEnumerator ReturnToRest()
    {
        while (Vector3.Distance(transform.localScale, baseScale) > 0.001f
            || Vector3.Distance(transform.localPosition, basePosition) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * easeSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePosition, Time.deltaTime * easeSpeed);
            yield return null;
        }

        transform.localScale = baseScale;
        transform.localPosition = basePosition;
        motionRoutine = null;
    }

    public void ClearOutline()
    {
        hovered = false;
        selected = false;
        activeTurn = false;
        isDefending = false;
        RefreshTint();
    }

    private void RefreshTint()
    {
        if (renderers == null) return;

        if (!IsAlive)
        {
            Apply(deadTint, true);
            return;
        }

        if (isDefending) Apply(defendTint, true);
        else if (selected) Apply(selectedTint, true);
        else if (hovered) Apply(hoverTint, true);
        else if (activeTurn) Apply(activeTurnTint, true);
        else Apply(Color.white, false);
    }

    private void Apply(Color tint, bool useTint)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (useTint)
            {
                Color b = baseColors[i];
                renderers[i].color = new Color(b.r * tint.r, b.g * tint.g, b.b * tint.b, b.a * tint.a);
            }
            else
            {
                renderers[i].color = baseColors[i];
            }
        } 
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        if (isDefending)
        {
            amount = Mathf.Max(0, amount / 2);
            isDefending = false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (!IsAlive) Die();
        else RefreshTint();
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void Die()
    {
        Debug.Log("[Combatant] " + name + " died.", this);

        hovered = false;
        selected = false;
        activeTurn = false;
        RefreshTint();

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
        {
            col.enabled = false;
        }
    }
}