using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateMachine : MonoBehaviour
{
    protected BattleManager battleManager;

    public enum TurnState
    {
        Idle,
        Choosing,
        Acting,
        Dead
    }
    public TurnState turnState;

    public string unitName;

    public float currentHP, maxHP, baseATK, currentATK, baseDEF, currentDEF, speed, stateCharge, animationSpeed;
    public int fireTokens, waterTokens, earthTokens, skyTokens;
    public bool dualState, alive, attackStarted;

    public double initiative;

    public List<Attack> attackList = new List<Attack>();
    public AttackCommand myAttack;
    public Sprite portrait;

    private void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        animationSpeed = battleManager.animationSpeed;
        turnState = TurnState.Idle;
    }

    private void Update()
    {
        switch (turnState) {
            case TurnState.Idle:

                break;

            case TurnState.Choosing:

                ChooseAction();
                break;

            case TurnState.Acting:

                StartCoroutine(StartAttack());
                break;

            case TurnState.Dead:

                break;
        }
    }

    protected virtual void ChooseAction()
    {
        myAttack = new AttackCommand();
        myAttack.target = battleManager.heroesInBattle[Random.Range(0, battleManager.heroesInBattle.Count)];
        //myAttack.chosenAttack = attackList[Random.Range(0, attackList.Count)];

        turnState = TurnState.Acting;
    }

    private IEnumerator MoveToTarget(Vector2 target)
    {
        while (MoveTick(target)) { yield return null; }
    }

    private bool MoveTick(Vector2 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, animationSpeed * Time.deltaTime);

        float xDifference = transform.position.x - target.x;
        float yDifference = transform.position.y - target.y;
        bool hasArrived = (Mathf.Approximately(xDifference, 0) && Mathf.Approximately(yDifference, 0));
        return !hasArrived;
    }

    private IEnumerator StartAttack()
    {
        if (attackStarted) {
            yield break;
        }
        attackStarted = true;

        Vector2 startPosition = transform.position;

        IEnumerator coroutine = MoveToTarget(myAttack.target.transform.position);
        StartCoroutine(coroutine);

        yield return new WaitForSeconds(0.5f);
        StopCoroutine(coroutine);

        // DoDomage() {}

        initiative -= battleManager.turnThreshold;

        battleManager.MoveToTarget(gameObject, startPosition);

        attackStarted = false;
        turnState = TurnState.Idle;
        battleManager.battleState = BattleManager.BattleState.AdvanceTime;
    }
}
