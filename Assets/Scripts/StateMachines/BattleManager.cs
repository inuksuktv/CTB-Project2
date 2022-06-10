using System;
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
    private RectTransform canvas;
    [SerializeField] private GameObject turnQueueSpacer;
    private Transform[] battleMarkers;
    private List<GameObject> enemiesNearby = new List<GameObject>();


    // Battle combatant lists.
    public List<GameObject> heroesInBattle = new List<GameObject>();
    public List<GameObject> enemiesInBattle = new List<GameObject>();
    public List<GameObject> combatants = new List<GameObject>();

    // Turn logic.
    public CachedInitiative[] unitInitiatives;
    public CachedInitiative[] cacheCopy;
    public List<GameObject> turnQueue = new List<GameObject>();

    // Battle logic knobs.
    public readonly float animationSpeed = 10f;
    public readonly float turnThreshold = 100f;
    public readonly int turnQueueSize = 10;

    private bool combatStarted = false;

    void Start()
    {
        // Get the battleMarker locations which are children of the battleManager. Children 0, 1, 2, 3 are always the heroes.
        battleMarkers = new Transform[transform.childCount];
        for (int i = 0; i < battleMarkers.Length; i++) {
            battleMarkers[i] = transform.GetChild(i);
        }

        battleGUI = GetComponent<BattleGUIManager>();
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        battleState = BattleState.Idle;
    }

    void Update()
    {
        switch (battleState) {
            case BattleState.Idle:

                break;

            case BattleState.AdvanceTime:

                if (combatants.Count == 0) {
                    battleState = BattleState.Idle;
                    break;
                }

                // Advance the battle state to the next turn. At the end of this block, the list of unitInitiatives is sorted with the actor at the front with 100 initiative.
                PrepareInitiative();
                CalculateTicksToNextTurn();
                AdvanceTime();

                // Simulate time and generate a turnQueue. This makes no changes to unitInitiatives.
                GenerateTurnQueue();
                battleGUI.ReceiveTurnQueue(turnQueue);

                // Tell the actor to take its turn and wait until it says it's done.
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
        StartCoroutine(StartCombat(collider.gameObject));
    }


    public IEnumerator StartCombat(GameObject unit)
    {
        // Set a flag so we only start combat once.
        if (combatStarted) {
            yield break;
        }
        combatStarted = true;

        unit.gameObject.GetComponent<PlayerController>().enabled = false;

        // I'm turning off rigidbodies as well so things don't bump into eachother.
        LoadCombatants();
        yield return StartCoroutine(MoveUnitsToMarkers());

        GameObject turnQueue = Instantiate(turnQueueSpacer, canvas);
        turnQueue.name = "TurnQueueSpacer";

        battleGUI.enabled = true;
        battleState = BattleState.AdvanceTime;
    }



    private void LoadCombatants()
    {
        // Load heroes.
        foreach (GameObject hero in GameManager.Instance.heroes) {
            heroesInBattle.Add(hero);
            combatants.Add(hero);
        }

        // Load enemies within 20 units.
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

        // Also disable rigidbodies?
        foreach (GameObject unit in combatants) {
            Destroy(unit.GetComponent<Rigidbody2D>());
        }
    }

    // Move each unit to its mark. Wait until they've all arrived.
    private IEnumerator MoveUnitsToMarkers()
    {
        Coroutine[] coroutines = new Coroutine[combatants.Count];

        for (int i = 0; i < coroutines.Length; i++) {
            GameObject unit = combatants[i];
            Vector2 mark = battleMarkers[i].position;
            coroutines[i] = StartCoroutine(MoveToTarget(unit, mark));
        }

        for (int i = 0; i < coroutines.Length; i++) {
            yield return coroutines[i];
        }
    }

    // Move a unit until it arrives at its target.
    public IEnumerator MoveToTarget(GameObject unit, Vector2 target)
    {
        yield return new WaitUntil(() => MoveTick(unit, target));
    }

    // Move the unit. Return true when the unit arrives.
    private bool MoveTick(GameObject unit, Vector2 target)
    {
        unit.transform.position = Vector2.MoveTowards(unit.transform.position, target, animationSpeed * Time.deltaTime);

        float xDifference = unit.transform.position.x - target.x;
        float yDifference = unit.transform.position.y - target.y;
        bool hasArrived = Mathf.Approximately(xDifference, 0) && Mathf.Approximately(yDifference, 0);
        return hasArrived;
    }

    // Cache each combatant's current speed and initiative.
    private void PrepareInitiative()
    {
        unitInitiatives = new CachedInitiative[combatants.Count];

        // Read each combatant's data into a new member of unitInitiatives.
        for (int i = 0; i < unitInitiatives.Length; i++) {
            CachedInitiative currentCache = new CachedInitiative();
            UnitStateMachine script = combatants[i].GetComponent<UnitStateMachine>();

            currentCache.unit = combatants[i];
            currentCache.initiative = script.initiative;
            currentCache.speed = script.speed;
            unitInitiatives[i] = currentCache;
        }
    }

    // Sort units by ticks needed.
    private void CalculateTicksToNextTurn()
    {
        // Calculate how many ticks each unit needs to fill its initiative.
        foreach (CachedInitiative unit in unitInitiatives) {
            double initiativeDifference = turnThreshold - unit.initiative;
            unit.ticksToNextTurn = initiativeDifference / unit.speed;
        }

        // Sort the array by ticks. Least ticks first.
        Array.Sort(unitInitiatives, delegate (CachedInitiative a, CachedInitiative b) {
            return a.ticksToNextTurn.CompareTo(b.ticksToNextTurn);
        });
    }

    // Increase each unit's initiative to bring up the next turn. Also increases the cached initiative.
    private void AdvanceTime()
    {
        if (unitInitiatives[0].initiative >= turnThreshold) { return; }
        else {
            double ticksToAdvance = unitInitiatives[0].ticksToNextTurn;
            foreach (CachedInitiative cache in unitInitiatives) {
                UnitStateMachine script = cache.unit.GetComponent<UnitStateMachine>();
                script.initiative += script.speed * ticksToAdvance;
                cache.initiative += script.speed * ticksToAdvance;
            }
        }
    }

    // This method copies unitInitiatives to simulate turns and assumes the list is already sorted with the first entry at 100 initiative.
    private void GenerateTurnQueue()
    {
        cacheCopy = new CachedInitiative[unitInitiatives.Length];
        Array.Copy(unitInitiatives, cacheCopy, cacheCopy.Length);

        turnQueue.Clear();

        // Populate the turnQueue using the copied array to simulate turns.
        while (turnQueue.Count < turnQueueSize) {

            // Simulate taking a turn. The first entry is ready at 100 initiative.
            turnQueue.Add(cacheCopy[0].unit);
            cacheCopy[0].initiative -= turnThreshold;

            // Update ticksToNextTurn and sort.
            foreach (CachedInitiative cache in cacheCopy) {
                double initiativeDifference = turnThreshold - cache.initiative;
                cache.ticksToNextTurn = initiativeDifference / cache.speed;
            }
            Array.Sort(cacheCopy, delegate (CachedInitiative a, CachedInitiative b) {
                return a.ticksToNextTurn.CompareTo(b.ticksToNextTurn);
            });

            // Advance initiative to the next turn.
            if (cacheCopy[0].initiative >= turnThreshold) { continue; }
            else {
                double ticksToAdvance = cacheCopy[0].ticksToNextTurn;
                foreach (CachedInitiative cache in cacheCopy) {
                    cache.initiative += cache.speed * ticksToAdvance;
                }
            }
        }
    }
}