using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Idle,
        AdvanceTime,
        VictoryCheck,
        Win,
        Lose
    }
    public BattleState battleState;

    private BattleGUIManager battleGUI;
    private Transform[] heroMarkers = new Transform[4];
    private List<Transform> enemyMarkers = new List<Transform>();
    private List<GameObject> enemiesNearby = new List<GameObject>();

    public List<GameObject> heroesInBattle = new List<GameObject>();
    public List<GameObject> enemiesInBattle = new List<GameObject>();
    public List<GameObject> combatants = new List<GameObject>();

    public List<CachedInitiative> unitInitiatives = new List<CachedInitiative>();

    public float animationSpeed = 5f;
    public float turnThreshold = 100f;

    private bool combatStarted = false;

    void Start()
    {
        for (int i = 0; i < 4; i++) {
            heroMarkers[i] = transform.GetChild(i);
        }
        for (int i = 0; i < 6; i++) {
            enemyMarkers.Add(transform.GetChild(i + 4));
        }

        battleGUI = GetComponent<BattleGUIManager>();
        battleState = BattleState.Idle;
    }

    void Update()
    {
        switch (battleState) {
            case BattleState.Idle:

                break;

            case BattleState.AdvanceTime:

                if (combatants.Count == 0) { battleState = BattleState.Idle; }

                PrepareInitiative();

                GenerateQueue();

                AdvanceTime();

                battleGUI.ReceiveTurnQueue(unitInitiatives);

                UnitStateMachine actor = unitInitiatives[0].unit.GetComponent<UnitStateMachine>();
                actor.turnState = UnitStateMachine.TurnState.Choosing;
                battleState = BattleState.Idle;

                break;

            case BattleState.VictoryCheck:

                break;

            case BattleState.Win:

                break;

            case BattleState.Lose:

                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        StartCombat(collider.gameObject);
    }


    public void StartCombat(GameObject unit)
    {
        // Set a flag so we only start combat once.
        if (combatStarted) {
            return;
        }
        combatStarted = true;

        unit.gameObject.GetComponent<PlayerController>().enabled = false;

        LoadCombatants();

        MoveUnitsToMarkers();

        battleGUI.enabled = true;
        battleGUI.heroGUI = BattleGUIManager.HeroGUI.Available;

        battleState = BattleState.AdvanceTime;
    }



    private void LoadCombatants()
    {
        enemiesNearby.Clear();
        heroesInBattle.Clear();
        enemiesInBattle.Clear();
        combatants.Clear();

        foreach (GameObject hero in GameManager.Instance.heroes) {
            heroesInBattle.Add(hero);
            combatants.Add(hero);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 20f);

        foreach (Collider2D collider in hitColliders) {
            if (collider.gameObject.CompareTag("Enemy")) {
                enemiesNearby.Add(collider.gameObject);
            }
        }

        foreach (GameObject enemy in enemiesNearby) {
            enemiesInBattle.Add(enemy);
            combatants.Add(enemy);
        }
    }

    // Start a coroutine for each unit to move to its marker.
    private void MoveUnitsToMarkers()
    {
        for (int i = 0; i < 4; i++) {
            GameObject hero = heroesInBattle[i];
            Transform mark = heroMarkers[i];
            IEnumerator coroutine = MoveToTarget(hero, mark.position);
            StartCoroutine(coroutine);
        }

        for (int i = 0; i < enemiesInBattle.Count; i++) {
            GameObject enemy = enemiesInBattle[i];
            Transform mark = enemyMarkers[i];
            IEnumerator coroutine = MoveToTarget(enemy, mark.position);
            StartCoroutine(coroutine);
        }
    }

    // Move a unit until it arrives at its target.
    public IEnumerator MoveToTarget(GameObject unit, Vector2 target)
    {
        yield return new WaitUntil(() => MoveTick(unit, target));
    }

    // Move the unit then return false when the unit arrives to end the parent coroutine.
    private bool MoveTick(GameObject unit, Vector2 target)
    {
        unit.transform.position = Vector2.MoveTowards(unit.transform.position, target, animationSpeed * Time.deltaTime);

        float xDifference = unit.transform.position.x - target.x;
        float yDifference = unit.transform.position.y - target.y;
        bool hasArrived = (Mathf.Approximately(xDifference, 0) && Mathf.Approximately(yDifference, 0));
        return hasArrived;
    }

    private void PrepareInitiative()
    {
        unitInitiatives.Clear();

        // Read each combatant's data into a new member of unitInitiatives.
        foreach (GameObject combatant in combatants) {
            CachedInitiative currentCache = new CachedInitiative();
            UnitStateMachine script = combatant.GetComponent<UnitStateMachine>();

            currentCache.unit = combatant;
            currentCache.initiative = script.initiative;
            currentCache.speed = script.speed;
            unitInitiatives.Add(currentCache);
        }
    }

    private void GenerateQueue()
    {
        // Calculate how many ticks each unit needs to fill its initiative.
        foreach (CachedInitiative actor in unitInitiatives) {
            double initiativeDifference = turnThreshold - actor.initiative;
            actor.ticks = initiativeDifference / actor.speed;
        }

        // Sort the cache by ticks. Lowest ticks first.
        unitInitiatives.Sort(delegate (CachedInitiative a, CachedInitiative b) {
            return a.ticks.CompareTo(b.ticks);
        });
    }

    private void AdvanceTime()
    {
        if (unitInitiatives[0].initiative >= turnThreshold) { return; }
        else {
            double ticksToAdvance = unitInitiatives[0].ticks;
            foreach (CachedInitiative cache in unitInitiatives) {
                UnitStateMachine script = cache.unit.GetComponent<UnitStateMachine>();
                script.initiative += script.speed * ticksToAdvance;
                cache.initiative += script.speed * ticksToAdvance;
            }
        }
    }
}
