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

    private bool MoveToTarget(Vector3 target)
    {
        Debug.Log("Moving to " + myAttack.target.name);
        transform.position = Vector3.MoveTowards(transform.position, target, animationSpeed * Time.deltaTime);
        return !(Mathf.Approximately(transform.position.x - target.x, 0) && Mathf.Approximately(transform.position.y - target.y, 0));
    }

    private IEnumerator StartAttack()
    {
        if (attackStarted) {
            yield break;
        }
        attackStarted = true;

        Vector2 startPosition = transform.position;

        // Calculate a target position to end up 3 units away from the attack target.
        Vector3 direction = (transform.position - myAttack.target.transform.position).normalized;
        Vector3 targetPosition = myAttack.target.transform.position + (3 * direction);
        while (MoveToTarget(targetPosition)) { yield return null; }

        // DoDomage() {}

        initiative -= battleManager.turnThreshold;

        while (MoveToTarget(startPosition)) { yield return null; }

        attackStarted = false;
        turnState = TurnState.Idle;
        battleManager.battleState = BattleManager.BattleState.AdvanceTime;
    }
}
