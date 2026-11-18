using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class EnemyAttackingState : EnemyBaseState
{
    private readonly IAttackBehaviour _behavior;

    public EnemyAttackingState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        _behavior = stateMachine.AttackBehaviour;
    }

    public override void Enter()
    {


        _behavior?.OnEnter();
        stateMachine.enemyWeapon.SetAttack(stateMachine.attackDamage, stateMachine.AttackKnockback);



    }
    public override void Tick(float deltaTime)
    {
        _behavior?.Tick(deltaTime);
        AvoidNearbyEnemies(5f, 7f, deltaTime);



    }

    // ---- Force helpers ----
    public override void HandleRootMotion(Vector3 rootMotionDelta, float deltaTime)
    {
        _behavior?.HandleRootMotion(rootMotionDelta, deltaTime);
    }


    public override void Exit()
    {
        _behavior?.OnExit();


    }

}
