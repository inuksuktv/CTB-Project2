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
}
