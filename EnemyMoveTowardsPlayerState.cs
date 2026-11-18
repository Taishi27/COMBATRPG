using UnityEngine;

public class EnemyMoveTowardsPlayerState : EnemyBaseState
{
    private float animatorDampTime = 0.1f;

    public readonly int AnimHash = Animator.StringToHash("Forward");
    public EnemyMoveTowardsPlayerState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entered EnemyMoveForward without slot to attack again bro");
        stateMachine.enemyAnimator.CrossFadeInFixedTime(AnimHash, animatorDampTime);
    }
    public override void Tick(float deltaTime)
    {
        AvoidNearbyEnemies(2.5f, 2f, deltaTime);
        //EvaluateNextCombatPhase();




        Vector3 toPlayer = stateMachine.playerHealth.transform.position - stateMachine.transform.position;
        toPlayer.y = 0;

        float offsetDistance = 2f;

        // Each enemy gets a unique angle based on their instance ID or index
        int seed = stateMachine.GetInstanceID(); // or slot index from CombatCoordinator
        float angleOffset = Mathf.Sin(seed * 0.1f) * 60f; // Randomish angle

        Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
        Vector3 offsetDir = rotation * -stateMachine.playerHealth.transform.forward;

        Vector3 targetPos = stateMachine.playerHealth.transform.position + offsetDir.normalized * offsetDistance;
        Vector3 moveDir = (targetPos - stateMachine.transform.position).normalized * stateMachine.EngagementSpeed;

        Vector3 separation = GetSeparationForce();
        stateMachine.EnemyFinalMove(moveDir + separation, deltaTime);
        FacePlayer();

        if (stateMachine.IsInAttackRange())
        {
            stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
        }
            

        if (!IsInEngagementRange())
        {
            stateMachine.SwitchState(new EnemyChasePlayerState(stateMachine));
            return;
        }
    }

    //private void EvaluateNextCombatPhase()
    //{
      

    //    switch (stateMachine.combatRole)
    //    {
    //        case CombatRoleType.Attacker:
    //            if (stateMachine.IsInAttackRange())
    //                stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
    //            break;

    //        case CombatRoleType.Pressure:
    //            stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));
    //            break;

    //        case CombatRoleType.Support:
    //            stateMachine.SetCombatState(new EnemyCirclingState(stateMachine));
    //            break;
    //    }
    //}
    public override void Exit()
    {
        
    }

   

 
}
