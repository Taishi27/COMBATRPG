using UnityEngine;

public class EnemyCirclingState : EnemyBaseState
{

    private float actionTimer;
    private float actionDuration;
    private int currentAnimHash;

    private Vector3 holdPosition;
    private readonly int idleHash = Animator.StringToHash("Support_Idle");
    private readonly int feintForwardHash = Animator.StringToHash("Support_FeintForward");
    private readonly int feintBackHash = Animator.StringToHash("Support_StepBack");

    public EnemyCirclingState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    private bool waitingForQueuedPromotion;
    private float promoteCooldown;



    public override bool UsesRootMotion => true;

    public override void Enter()
    {
        Debug.Log("circlin");

        stateMachine.enemyAnimator.applyRootMotion = true;


        stateMachine.IsActiveInCombat = true;

        waitingForQueuedPromotion = false;
        promoteCooldown = 0f;
        //if (stateMachine.currentSlot != null)
        //{
        //    holdPosition = stateMachine.currentSlot.WorldPosition(stateMachine.playerHealth.transform);
        //}
           
        //else
        //    holdPosition = stateMachine.transform.position; // fallback

        PickNewFeint();

    }

    public override void Tick(float deltaTime)
    {
        stateMachine.Move(deltaTime);
        FacePlayer();
        AvoidNearbyEnemies(4f, 4f, deltaTime);

        // your existing micro-movement around holdPosition...
        MaintainHoldPosition(deltaTime);

        // tiny cooldown so we don’t spam the manager each frame
       

        // If close enough to the player lane, try to promote


        // Your existing cadence checks
        actionTimer += deltaTime;
        if (actionTimer >= actionDuration)
        {
            PickNewFeint();
            //EvaluateNextCombatPhase(); // safe: uses current combatRole
        }
    }

    

    //private void PlayCircleAnimation(int direction)
    //{
    //    int hash = direction > 0 ? circleRightHash : circleLeftHash;
    //    stateMachine.enemyAnimator.CrossFadeInFixedTime(hash, 0.15f);
    //}

    //public override void HandleRootMotion(Vector3 rootMotionDelta, float deltaTime)
    //{
    //    Vector3 toPlayer = (CombatCoordinator.Instance.player.position - stateMachine.transform.position).normalized;
    //    Vector3 lateral = Vector3.Cross(Vector3.up, toPlayer).normalized * lateralDirection;

    //    Quaternion align = Quaternion.FromToRotation(Vector3.forward, lateral);
    //    Vector3 adjustedDelta = align * rootMotionDelta;

    //   EnemyFinalMove(adjustedDelta / deltaTime, deltaTime);
    //}

    private void PickNewFeint()
    {
        actionTimer = 0f;
        float decision = Random.value;

        if (decision < 0.4f)
        {
            currentAnimHash = idleHash;
            actionDuration = Random.Range(1.5f, 2.5f);
        }
        else if (decision < 0.7f)
        {
            currentAnimHash = feintForwardHash;
            actionDuration = 0.8f;
        }
        else
        {
            currentAnimHash = feintBackHash;
            actionDuration = 0.8f;
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
    //        case CombatRoleType.PrimaryAttacker:
    //            if (stateMachine.IsInAttackRange())
    //                stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
    //            else
    //                stateMachine.SetCombatState(new EnemyReEngageForwardState(stateMachine));
    //            break;

    //        case CombatRoleType.SecondaryAttacker:
    //            stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));
    //            break;

    //        case CombatRoleType.Support:
    //            stateMachine.SetCombatState(new EnemyCirclingState(stateMachine));
    //            break;
    //    }
    //}
 

 
    private bool IsPlayerWithinFlankerPromoteDistance()
    {
        float d = Vector3.Distance(stateMachine.transform.position,
                                   CombatPhaseManager.Instance.player.position);
        return d <= /* mirror manager value or keep your own */ 4.0f;
    }

    private void MaintainHoldPosition(float dt)
    {
        float dist = Vector3.Distance(stateMachine.transform.position, holdPosition);
        if (dist > 6f)
        {
            Vector3 toHold = (holdPosition - stateMachine.transform.position).normalized;
            stateMachine.EnemyFinalMove(toHold * stateMachine.EngagementSpeed, dt);
        }
    }

    public override void Exit()
    {
        stateMachine.enemyAnimator.applyRootMotion = false;
    }
}

