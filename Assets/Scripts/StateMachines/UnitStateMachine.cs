using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateMachine : MonoBehaviour
{
    protected BattleManager battleManager;
    public Animator animator;

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
    public bool dualState, attackStarted, delayedAttack, animationComplete;
    protected bool alive = true;
    public double initiative;

    public bool isGuarded, isEvading, isRegenerating, isBurning, isVulnerable;

    public List<Attack> attackList = new List<Attack>();
    public AttackCommand myAttack;
    public Sprite portrait;
    public GameObject guard;
    private MOBAEnergyBar healthBar;

    private void Start()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        animator = GetComponent<Animator>();
        animationSpeed = battleManager.animationSpeed;
        turnState = TurnState.Idle;

        ScreenSpacePanel screenSpacePanel = GetComponentInChildren<ScreenSpacePanel>();
        healthBar = screenSpacePanel.PanelUIElement.GetComponentInChildren<MOBAHealthBarPanel>().HealthBar;
        healthBar.MaxValue = maxHP;
        healthBar.SetValueNoBurn(currentHP);
    }

    private void Update()
    {
        switch (turnState)
        {
            case TurnState.Idle:

                break;

            case TurnState.Choosing:

                StartTurn();
                ChooseAction();

                break;

            case TurnState.Acting:

                StartCoroutine(StartAttack());

                break;

            case TurnState.Dead:

                DieAndCleanup();

                break;
        }
    }

    public void CollectAction(AttackCommand attack)
    {
        myAttack = attack;
        turnState = TurnState.Acting;
    }

    public void TakeDamage(float damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        healthBar.Value = currentHP;
        if (currentHP == 0) {
            turnState = TurnState.Dead;
        }
        Debug.Log(gameObject.name + " took " + damage + " damage.");

        string damagePopup = damage.ToString();
        Vector3 popupPosition = transform.position;
        battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(damagePopup, popupPosition);
    }

    public void TakeStatDamage(float damage)
    {
        initiative = Mathf.Clamp((float)initiative - damage, 0, battleManager.turnThreshold * 2);
        Debug.Log(gameObject.name + " 's initiative reduced by " + damage);

        string damagePopup = damage.ToString();
        Vector3 popupPosition = transform.position;
        battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(damagePopup, popupPosition);
    }

    protected virtual void ChooseAction()
    {
        Attack attack = attackList[Random.Range(0, attackList.Count)];

        myAttack = ScriptableObject.CreateInstance<AttackCommand>();
        myAttack.attacker = gameObject;
        myAttack.target = battleManager.heroesInBattle[Random.Range(0, battleManager.heroesInBattle.Count)];
        // Choose a new target if the current one is Phased.
        if (myAttack.target.GetComponent<UnitStateMachine>().delayedAttack) {
            List<GameObject> targets = battleManager.heroesInBattle;
            targets.Remove(myAttack.target);
            if (targets.Count == 0) {
                EndTurn();
                return;
            }
            else {
                myAttack.target = targets[Random.Range(0, targets.Count)];
            }
        }

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

    protected virtual void DieAndCleanup()
    {
        if (!alive) { return; }
        else {
            tag = "DeadEnemy";

            battleManager.enemiesInBattle.Remove(gameObject);
            battleManager.combatants.Remove(gameObject);

            GetComponent<SpriteRenderer>().color = Color.black;

            // Turn off particle effects here as well.

            alive = false;

            battleManager.battleState = BattleManager.BattleState.VictoryCheck;
        }
    }

    private void AnimationFinished()
    {
        animationComplete = true;
    }

    private IEnumerator DelayedAttack()
    {
        Vector2 startPosition = transform.position;

        // Play an animation and vanish.
        if (animator != null) {
            animator.SetTrigger("Fold");
            animationComplete = false;
            while (!animationComplete) { yield return null; }
        }
        GetComponent<SpriteRenderer>().enabled = false;

        // Set initiative and delayedAttack flag.
        delayedAttack = true;
        initiative -= battleManager.turnThreshold / 2;

        // Tell the battle manager to send the next turn.
        battleManager.battleState = BattleManager.BattleState.AdvanceTime;
        turnState = TurnState.Idle;

        // Wait until this unit's turn comes up again.
        yield return new WaitWhile(() => delayedAttack);
        turnState = TurnState.Acting;

        // Pick a new target if the old target is dead.
        if (myAttack.target.CompareTag("DeadEnemy")) {
            myAttack.target = battleManager.enemiesInBattle[Random.Range(0, battleManager.enemiesInBattle.Count)];
        }

        // Appear at a new location.
        transform.position = myAttack.target.transform.position + Vector3.up;
        GetComponent<SpriteRenderer>().enabled = true;
        if (animator != null) {
            animator.SetTrigger("Fold");
            animationComplete = false;
            while (!animationComplete) { yield return null; }
        }

        // Complete the attack.
        DoDamage();
        yield return MoveToTarget(startPosition);
        EndTurn();
    }

    private void DoDamage()
    {
        UnitStateMachine defender = myAttack.target.GetComponent<UnitStateMachine>();

        // Look for any special handling before the attack. Guard had to be moved outside this method, earlier in the attack coroutine.
        if (defender.isEvading) {
            string textPopup = "Evade!";
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, defender.transform.position);

            defender.isEvading = false;
            return;
        }

        // Apply or remove tokens.

        // Calculate raw attack damage. 
        float calcDamage = myAttack.damage + currentATK;

        // Defender mitigates. Shields subtract from damage along with DEF here.
        calcDamage -= defender.currentDEF;

        // Post-mitigation effects. Absorption applies here.
        if (defender.isVulnerable) {
            calcDamage *= 1.5f;
            defender.isVulnerable = false;
        }
        if (myAttack.damageMode == Attack.DamageMode.Flash) {
            float healthRatio = currentHP / maxHP;
            calcDamage *= (2 - healthRatio);
        }

        // Send damage.
        if (myAttack.damageMode == Attack.DamageMode.StatDamage) {
            defender.TakeStatDamage(Mathf.Floor(calcDamage));
        }
        else {
            defender.TakeDamage(Mathf.Floor(calcDamage));
        }

        if (myAttack.damageMode == Attack.DamageMode.Burn) {
            TakeDamage(Mathf.Floor(calcDamage / 2));
        }

        // Set status.
        if (myAttack.setStatus == Attack.SetStatus.Burning) {
            defender.isBurning = true;
            string textPopup = "Burning!";
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, defender.transform.position);
            // Play particleEffect for Burn.
        }
        else if (myAttack.setStatus == Attack.SetStatus.Evasion) {
            defender.isEvading = true;
            // Play particleEffect(?) for Evade.
        }
        else if (myAttack.setStatus == Attack.SetStatus.Regen) {
            defender.isRegenerating = true;
            string textPopup = "Regen";
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, defender.transform.position);
            // Play particleEffect for Regen.
        }
        else if (myAttack.setStatus == Attack.SetStatus.Vulnerable) {
            defender.isVulnerable = true;
            string textPopup = "Vulnerable!";
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, defender.transform.position);
            // Play particleEffect(?) for Vulnerable.
        }
        else if (myAttack.setStatus == Attack.SetStatus.Reset) {
            defender.isEvading = false;
            defender.isRegenerating = false;
            defender.isBurning = false;
            defender.isVulnerable = false;
        }
        else if (myAttack.setStatus == Attack.SetStatus.Guard) {
            defender.isGuarded = true;
            defender.GetComponent<UnitStateMachine>().guard = gameObject;
        }

        // Anything else?
    }

    private void EndTurn()
    {
        if (isBurning) {
            float burnAmount = 20;
            currentHP = Mathf.Clamp(currentHP - burnAmount, 1, maxHP);
            string textPopup = burnAmount.ToString();
            Vector3 popupPosition = transform.position;
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, popupPosition);
        }

        attackStarted = false;
        initiative -= battleManager.turnThreshold;
        turnState = TurnState.Idle;
        battleManager.battleState = BattleManager.BattleState.AdvanceTime;
    }

    private IEnumerator GuardedAttack(Vector2 attackerStartPosition)
    {
        // Move the guard and animate him.
        UnitStateMachine defenderScript = myAttack.target.GetComponent<UnitStateMachine>();
        GameObject guard = defenderScript.guard;
        Vector2 guardStartPosition = guard.transform.position;

        guard.transform.position = Vector2.Lerp(transform.position, myAttack.target.transform.position, 0.5f);

        myAttack.target = guard;
        defenderScript.isGuarded = false;
        defenderScript.guard = null;

        DoDamage();
        // This will look better if we wait until the blocking animation is done before continuing.
        guard.GetComponent<UnitStateMachine>().animator.SetTrigger("Fold");
        yield return MoveToTarget(attackerStartPosition);
        guard.transform.position = guardStartPosition;
        EndTurn();
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

        if (myAttack.damageMode == Attack.DamageMode.Delayed) {
            StartCoroutine(DelayedAttack());
            yield break;
        }

        Vector2 startPosition = transform.position;

        // Calculate a target position halfway to the target. Wait until the unit arrives.
        Vector2 targetPosition = Vector2.Lerp(transform.position, myAttack.target.transform.position, 0.5f);
        yield return MoveToTarget(targetPosition);

        if (animator != null) {
            animator.SetTrigger("Fold");
            animationComplete = false;
            while (!animationComplete) { yield return null; }
        }

        if (myAttack.target.GetComponent<UnitStateMachine>().isGuarded && myAttack.targetMode == Attack.TargetMode.Enemies) {
            StartCoroutine(GuardedAttack(startPosition));
            yield break;
        }

        DoDamage();
        yield return MoveToTarget(startPosition);
        EndTurn();
    }

    private void StartTurn()
    {
        if (delayedAttack) { return; }
        if (isRegenerating) {
            float regenAmount = 20;
            currentHP = Mathf.Clamp(currentHP + regenAmount, 0, maxHP);
            string textPopup = regenAmount.ToString();
            Vector3 popupPosition = transform.position;
            battleManager.gameObject.GetComponent<BattleGUIManager>().TextPopup(textPopup, popupPosition);
        }
    }

    private IEnumerator MoveToTarget(Vector2 target)
    {
        yield return new WaitUntil(() => MoveTick(target));
    }
}