using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnOrderMode
{
    Manual,
    ByPriority,
    BySpeed
}

public enum BattleState
{
    ChoosingTarget,
    Resolving,
    Victory,
    Defeat
}

public class CombatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TargetSelector targetSelector;

    [Header("Input")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Z;

    [Header("Turn Order")]
    [SerializeField] private TurnOrderMode orderMode = TurnOrderMode.Manual;
    [SerializeField] private List<Combatant> manualOrder = new List<Combatant>();
    [SerializeField] private bool appendUnlisted = true;

    [Header("Timing")]
    [SerializeField] private float resolveDelay = 0.4f;
    [SerializeField] private float enemyThinkDelay = 0.6f;

    [Header("Runtime")]
    [SerializeField] private BattleState state;
    [SerializeField] private List<Combatant> turnOrder = new List<Combatant>();
    [SerializeField] private int currentIndex = -1;
    [SerializeField] private Combatant activeCombatant;
    private bool isTransitioning;

    [SerializeField] private GameObject Victory; 
    [SerializeField] private GameObject Defeat; 
    [SerializeField] private AudioClip VictorySound;
    [SerializeField] private AudioClip GameOver;

    private const string LevelProgressKey = "CurrentLevelIndex";
    private readonly string[] levelScenes = { "First Level", "Second Level", "Third Level", "Boss Level" };

    void Start()
    {
        Debug.Log("[CombatHandler] Start called.");

        SyncLevelProgressToCurrentScene();

        if (targetSelector == null)
        {
            Debug.LogError("[CombatHandler] Target Selector is not assigned. Drag this object into that field.", this);
            return;
        }

        StartFight();
    }

    void Update()
    {
        if (state != BattleState.ChoosingTarget) return;
        if (Input.GetKeyDown(KeyCode.X))
        {
            activeCombatant.SetDefending(true);
            targetSelector.Cancel();
            state = BattleState.Resolving;
            NextTurn(); // Skip to next turn
            return;
        }
        if (!Input.GetKeyDown(confirmKey)) return;

        Combatant target = targetSelector.Selected;
        if (target == null || !target.IsAlive) return;

        targetSelector.Cancel();
        state = BattleState.Resolving;
        StartCoroutine(Resolve(activeCombatant, target));
    }

    private void StartFight()
    {
        BuildTurnOrder();

        if (turnOrder.Count == 0)
        {
            Debug.LogError("No combatants in the scene");
            return;
        }

        string names = "";
        foreach (Combatant c in turnOrder) names += c.name + " (spd " + c.speed + ", party " + c.isPartyMember + ")  ";
        Debug.Log("[CombatHandler] Turn order: " + names);

        currentIndex = -1;
        NextTurn();
    }

    private void BuildTurnOrder()
    {
        Combatant[] all = FindObjectsByType<Combatant>(FindObjectsSortMode.None);
        turnOrder = new List<Combatant>();

        if (orderMode == TurnOrderMode.Manual)
        {
            foreach (Combatant listed in manualOrder)
            {
                if (listed != null && !turnOrder.Contains(listed)) turnOrder.Add(listed);
            }

            if (appendUnlisted)
            {
                foreach (Combatant extra in all.OrderByDescending(x => x.speed))
                {
                    if (!turnOrder.Contains(extra)) turnOrder.Add(extra);
                }
            }
        }
        else if (orderMode == TurnOrderMode.ByPriority)
        {
            turnOrder = all
                .OrderBy(x => x.turnPriority)
                .ThenByDescending(x => x.speed)
                .ThenBy(x => x.name)
                .ToList();
        }
        else
        {
            turnOrder = all
                .OrderByDescending(x => x.speed)
                .ThenByDescending(x => x.isPartyMember)
                .ThenBy(x => x.name)
                .ToList();
        }
    }

    [ContextMenu("Fill Manual Order From Scene")]
    private void FillManualOrderFromScene()
    {
        manualOrder = FindObjectsByType<Combatant>(FindObjectsSortMode.None)
            .OrderByDescending(x => x.isPartyMember)
            .ThenByDescending(x => x.speed)
            .ToList();
    }

    private void NextTurn()
    {
        if (activeCombatant != null) 
        {
            activeCombatant.SetActiveTurn(false);
        }
        if (CheckEndConditions()) return;

        do
        {
            currentIndex = (currentIndex + 1) % turnOrder.Count;
        }
        while (!turnOrder[currentIndex].IsAlive);

        activeCombatant = turnOrder[currentIndex];
        activeCombatant.SetActiveTurn(true);
        Debug.Log("[CombatHandler] Active combatant is now " + activeCombatant.name, activeCombatant);

        if (activeCombatant.isPartyMember) BeginPlayerTurn();
        else StartCoroutine(EnemyTurn());
    }

    private void BeginPlayerTurn()
    {
        List<Combatant> enemies = turnOrder
            .Where(c => !c.isPartyMember && c.IsAlive)
            .ToList();

        state = BattleState.ChoosingTarget;
        targetSelector.Begin(enemies);

        Debug.Log("[CombatHandler] " + activeCombatant.name + "'s turn. " + enemies.Count
            + " enemies alive. Click a target, press " + confirmKey + " to attack.");
    }

    private IEnumerator Resolve(Combatant attacker, Combatant target)
    {
        target.SetSelected(true);
        yield return attacker.PlayAttack(target);
        target.TakeDamage(attacker.strength);

        Debug.Log(attacker.name + " hits " + target.name + " for " + attacker.strength
            + ". " + target.name + " at " + target.CurrentHealth + "/" + target.MaxHealth);

        yield return new WaitForSeconds(resolveDelay);
        target.SetSelected(false);

        NextTurn();
    }

    private IEnumerator EnemyTurn()
    {
        state = BattleState.Resolving;
        yield return new WaitForSeconds(enemyThinkDelay);

        List<Combatant> party = turnOrder
            .Where(c => c.isPartyMember && c.IsAlive)
            .ToList();

        if (party.Count > 0)
        {
            Combatant target = party[Random.Range(0, party.Count)];
            target.SetSelected(true);
            yield return activeCombatant.PlayAttack(target);
            target.TakeDamage(activeCombatant.strength);

            Debug.Log(activeCombatant.name + " hits " + target.name + " for " + activeCombatant.strength
                + ". " + target.name + " at " + target.CurrentHealth + "/" + target.MaxHealth);

            yield return new WaitForSeconds(resolveDelay);
            target.SetSelected(false);
        }

        NextTurn();
    }

    private bool CheckEndConditions()
    {
        bool partyAlive = turnOrder.Any(c => c.isPartyMember && c.IsAlive);
        bool enemiesAlive = turnOrder.Any(c => !c.isPartyMember && c.IsAlive);

        if (!enemiesAlive)
        {
            state = BattleState.Victory;
            targetSelector.Cancel();
            Debug.Log("Victory");
            HideAllHealthBars();
            Victory.SetActive(true);
            Victory.GetComponent<AudioSource>().PlayOneShot(VictorySound, 1);
            StartCoroutine(ExampleCoroutine());
            return true;
        }

        if (!partyAlive)
        {
            state = BattleState.Defeat;
            targetSelector.Cancel();
            Debug.Log("Defeat");
            HideAllHealthBars();
            Defeat.SetActive(true);
            Defeat.GetComponent<AudioSource>().PlayOneShot(GameOver, 1);
            StartCoroutine(ExampleCoroutine2());
            return true;
        }

        return false;
    }

    private void HideAllHealthBars()
    {
        foreach (HealthBar bar in FindObjectsByType<HealthBar>(FindObjectsSortMode.None))
        {
            if (bar != null)
            {
                bar.SetVisible(false);
            }
        }
    }

    private void SyncLevelProgressToCurrentScene()
    {
        int sceneIndex = GetCurrentSceneIndex();
        if (sceneIndex >= 0)
        {
            PlayerPrefs.SetInt(LevelProgressKey, sceneIndex + 1);
            PlayerPrefs.Save();
        }
    }

    private int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (string.Equals(levelScenes[i], sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private int GetCurrentSceneIndex()
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex >= 0)
        {
            return sceneIndex;
        }

        int savedIndex = PlayerPrefs.GetInt(LevelProgressKey, 0);
        if (savedIndex > 0)
        {
            return Mathf.Clamp(savedIndex - 1, 0, levelScenes.Length - 1);
        }

        return 0;
    }

    private string GetNextSceneName()
    {
        int currentIndex = GetCurrentSceneIndex();
        int nextIndex = currentIndex + 1;
        if (nextIndex >= levelScenes.Length)
        {
            nextIndex = 0;
        }

        return levelScenes[nextIndex];
    }

    private void AdvanceLevelProgress()
    {
        int currentIndex = GetCurrentSceneIndex();
        int nextIndex = currentIndex + 1;
        if (nextIndex >= levelScenes.Length)
        {
            nextIndex = 0;
        }

        PlayerPrefs.SetInt(LevelProgressKey, nextIndex + 1);
        PlayerPrefs.Save();
    }

    IEnumerator ExampleCoroutine()
    {
        if (isTransitioning)
        {
            yield break;
        }

        isTransitioning = true;

        foreach (Combatant c in turnOrder) c.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        Victory.SetActive(false);
        AdvanceLevelProgress();
        SceneManager.LoadScene(GetNextSceneName());
    }

    IEnumerator ExampleCoroutine2()
    {
        foreach (Combatant c in turnOrder) c.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        Defeat.SetActive(false);
    }
}