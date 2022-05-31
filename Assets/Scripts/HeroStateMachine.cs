using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroStateMachine : UnitStateMachine
{
    protected override void ChooseAction()
    {
        base.ChooseAction();
        myAttack.target = battleManager.enemiesInBattle[Random.Range(0, battleManager.enemiesInBattle.Count)];
    }
}
