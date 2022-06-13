using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateMachine : MonoBehaviour
{
    protected BattleManager battleManager;
    protected Animator animator;

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
    public bool animationComplete;
    public double initiative;

    public bool isEvading;

    public List<Attack> attackList = new List<Attack>();
    public AttackCommand myAttack;
    public Sprite portrait;

    private void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        animator = GetComponent<Animator>();
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

    public void CollectAction(AttackCommand attack)
    {
        myAttack = attack;
        turnState = TurnState.Acting;
    }

    protected virtual void ChooseAction()
    {
        Attack attack = attackList[Random.Range(0, attackList.Count)];
        myAttack = ScriptableObject.CreateInstance<AttackCommand>();
        myAttack.attacker = gameObject;
        myAttack.target = battleManager.heroesInBattle[Random.Range(0, battleManager.heroesInBattle.Count)];
        myAttack.attackName = attack.attackName;
        myAttack.description = attack.description;
        myAttack.fireTokens = attack.fireTokens;
        myAttack.waterTokens = attack.waterTokens;
        myAttack.earthTokens = attack.earthTokens;
        myAttack.skyTokens = attack.skyTokens;
        myAttack.damage = attack.damage;
        myAttack.stateCharge = attack.stateCharge;
        myAttack.targetMode = attack.targetMode;
        myAttack.damageMode = attack.damageMode;
        myAttack.setStatus = attack.setStatus;

        turnState = TurnState.Acting;
    }

    private void DoDamage()
    {
        UnitStateMachine attacker = GetComponent<UnitStateMachine>();
        UnitStateMachine defender = myAttack.target.GetComponent<UnitStateMachine>();

        // Look for any special handling before the attack. Evade or Guard for example.
        if (defender.isEvading) {
            // "Evade!" text pop-up.
            Debug.Log("Evade!");

            defender.isEvading = false;
            return;
        }
        
        // Apply or remove tokens.

        // Calculate raw attack damage. 

        // Defender mitigates.

        // Post-mitigation effects.

        // Deal damage.

        // Set status. Need to read this enum into the attackCommand before it will work.
        if (myAttack.setStatus == AttackCommand.SetStatus.Evasion) {
            defender.isEvading = true;
        }

        // Anything else?
    }

    private IEnumerator MoveToTarget(Vector2 target)
    {
        yield return new WaitUntil(() => MoveTick(target));
    }

    // Move the unit. Return true when the unit arrives.
    private bool MoveTick(Vector2 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, animationSpeed * Time.deltaTime);

        float xDifference = transform.position.x - target.x;
        float yDifference = transform.position.y - target.y;
        bool hasArrived = Mathf.Approximately(xDifference, 0) && Mathf.Approximately(yDifference, 0);
        return hasArrived;
    }

    private IEnumerator StartAttack()
    {
        if (attackStarted) {
            yield break;
        }
        attackStarted = true;

        Vector2 startPosition = transform.position;

        // Calculate a target position to end up 3 units away from the attack target. Wait until the unit arrives.
        Vector3 direction = (transform.position - myAttack.target.transform.position).normalized;
        Vector3 targetPosition = myAttack.target.transform.position + (3 * direction);
        yield return MoveToTarget(targetPosition);

        // Trigger an animation. Wait by setting an event in the animation that sets animationComplete true.
        if (animator != null) {
            animator.SetTrigger("Fold");
        }
        animationComplete = false;
        if (animator != null) {
            while (!animationComplete) { yield return null; }
        }

        DoDamage();

        initiative -= battleManager.turnThreshold;

        yield return MoveToTarget(startPosition);

        attackStarted = false;
        turnState = TurnState.Idle;
        battleManager.battleState = BattleManager.BattleState.AdvanceTime;
    }

    private void AnimationFinished()
    {
        animationComplete = true;
    }
}