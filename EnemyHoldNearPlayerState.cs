using UnityEngine;

public class EnemyHoldNearPlayerState : EnemyBaseState
{
    private float holdTimer;
    //private float stepTimer;
    //private float nextStepTime;
    //private Vector3 currentStepVector;
    private float animatorDampTime = 0.1f;
    public override bool UsesRootMotion => true;
    public readonly int AnimHash = Animator.StringToHash("Repo");

    private float animTimer;
    private float animDuration;
    private int currentAnimHash;
    private readonly int idleAHash = Animator.StringToHash("Hold_Idle_A");
    private readonly int idleBHash = Animator.StringToHash("Hold_Idle_B");
    private readonly int feintHash = Animator.StringToHash("Hold_Feint");
    public EnemyHoldNearPlayerState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        //CombatPhaseManager.Instance.TryRegisterEnemy(stateMachine);
       
        Debug.Log("Entered EnemyHoldNearPlayerState");
        holdTimer = Random.Range(3f, 6f);
        //currentStepVector = Vector3.zero;
        //nextStepTime = Random.Range(0.15f, 0.35f); // Time before first step
        //stepTimer = 0f;
        stateMachine.enemyAnimator.applyRootMotion = true;
        stateMachine.IsActiveInCombat = true;
        stateMachine.enemyAnimator.CrossFadeInFixedTime(AnimHash, animatorDampTime);

        PickNewAnim();
    }

    public override void Tick(float deltaTime)
    {
        //holdTimer -= deltaTime;

        //Vector3 playerPos = CombatCoordinator.Instance.player.position;
        //Vector3 toPlayer = (playerPos - stateMachine.transform.position).normalized;

        //// Maintain spacing from player
        //float idealDistance = 2.2f;
        //Vector3 offsetDir = (stateMachine.transform.position - playerPos).normalized;
        //Vector3 distancePush = offsetDir * 0.5f * Mathf.Clamp01(idealDistance - Vector3.Distance(stateMachine.transform.position, playerPos));

        //// Fake micro steps every few seconds
        //stepTimer += deltaTime;
        //Vector3 step = Vector3.zero;
        //if (stepTimer >= nextStepTime)
        //{
        //    int type = Random.Range(0, 2);
        //    switch (type)
        //    {
        //        case 0: step = Vector3.Cross(Vector3.up, toPlayer); break;      // Step right
        //        case 1: step = -Vector3.Cross(Vector3.up, toPlayer); break;     // Step left

        //    }

        //    step = step.normalized * 0.4f;
        //    nextStepTime = Random.Range(2f, 4f);
        //    stepTimer = 0f;
        //}

        //Vector3 move = distancePush + step;
        //EnemyFinalMove(move, deltaTime);
        //FacePlayer();
        holdTimer -= deltaTime;
        //stepTimer += deltaTime;
        AvoidNearbyEnemies(4f, 4f, deltaTime);
        FacePlayer();

        // Optional: Repel if too close to player
        //Vector3 playerPos = CombatCoordinator.Instance.player.position;
        //float idealDistance = 3.5f;
        //float currentDistance = Vector3.Distance(stateMachine.transform.position, playerPos);

        //if (currentDistance < idealDistance - 0.5f)
        //{
        //    Vector3 repel = (stateMachine.transform.position - playerPos).normalized * 0.5f;
        //    EnemyFinalMove(repel, deltaTime); // small nudge backward
        //}


        animTimer += deltaTime;
        if (animTimer >= animDuration)
        {
            PickNewAnim();
        }

        //if (holdTimer <= 0f)
        //{
            
        //    //Debug.Log(decision);
        //    //EvaluateNextCombatPhase();
        //}


    
    }
    private void PickNewAnim()
    {
        animTimer = 0f;

        float decision = Random.value;
        if (decision < 0.4f)
        {
            currentAnimHash = idleAHash;
            animDuration = Random.Range(2f, 3f);
        }
        else if (decision < 0.8f)
        {
            currentAnimHash = idleBHash;
            animDuration = Random.Range(2f, 3f);
        }
        else
        {
            currentAnimHash = feintHash;
            animDuration = 1.1f;
        }

        stateMachine.enemyAnimator.CrossFadeInFixedTime(currentAnimHash, 0.1f);
    }

    //private void EvaluateNextCombatPhase()
    //{
    //    if (!IsInEngagementRange())
    //    {
    //        stateMachine.SwitchState(new EnemyChasePlayerState(stateMachine));
    //        return;
    //    }

    //    switch (stateMachine.combatRole)
    //    {
    //        case CombatRoleType.Attacker:
    //            if (stateMachine.IsInAttackRange())
    //                stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
    //            else
    //                stateMachine.SetCombatState(new EnemyReEngageForwardState(stateMachine));
    //            break;

    //        case CombatRoleType.Pressure:
    //            stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));
    //            break;

    //        case CombatRoleType.Support:
    //            stateMachine.SetCombatState(new EnemyCirclingState(stateMachine));
    //            break;
    //    }
    //}


    public override void HandleRootMotion(Vector3 rootMotionDelta, float deltaTime)
    {
      stateMachine.EnemyFinalMove(rootMotionDelta / deltaTime, deltaTime);
    }
    public override void Exit() 
    {
        stateMachine.enemyAnimator.applyRootMotion = false;
    }
}
