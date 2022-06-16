using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroStateMachine : UnitStateMachine
{
    protected override void ChooseAction()
    {
        myAttack = ScriptableObject.CreateInstance<AttackCommand>();
        turnState = TurnState.Idle;
    }

    protected override void DieAndCleanup()
    {
        if (!alive) { return; }
        else {
            tag = "DeadHero";

            battleManager.heroesInBattle.Remove(gameObject);
            battleManager.combatants.Remove(gameObject);

            GetComponent<SpriteRenderer>().color = Color.black;

            // Turn off particle effects here as well.
            // Maybe I should turn off the dead hero's input panel here?

            alive = false;

            battleManager.battleState = BattleManager.BattleState.VictoryCheck;
        }
    }
}
