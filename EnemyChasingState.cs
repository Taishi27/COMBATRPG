using System;
using UnityEngine;



public class EnemyChasingState : EnemyBaseState
{
    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("EnemySpeed");

    private float animatorDampTime = 0.1f;



    private float detectionRadius = 1.5f;
    private float detectionDistance = 3f;
    private int angleChecks = 6;
    private float maxAvoidanceAngle = 90f;

    private Vector3 lastValidDirection;
    private float rotationSpeed = 3f;
    private Vector3 previousMoveDirection = Vector3.forward; // or last direction
    private float directionLerpSpeed = 2f; // smooth turn

    private float avoidanceRecheckCooldown = 0.2f;
    private float lastAvoidanceCheckTime = 0f;
    private Vector3 cachedDesiredDirection;
    public EnemyChasingState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        CombatCoordinator.Instance.RegisterEnemyIntoCombat(stateMachine);
        //CombatPhaseManager.Instance.NotifyAttackFinished(stateMachine);
        stateMachine.IsActiveInCombat = true;


        //TryClaimSlot();
     
        stateMachine.enemyAnimator.CrossFadeInFixedTime(LocomotionHash, animatorDampTime);
        Debug.Log("Entering chasing player state");
        Vector3 toTarget = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).normalized;
        previousMoveDirection = toTarget;
        cachedDesiredDirection = toTarget;
        lastAvoidanceCheckTime = Time.time;
        //CombatPhaseManager.Instance.Suspend(stateMachine, 0f, SuspendReason.Distance);
    }

    public override void Tick(float deltaTime)
    {
        stateMachine.enemyAnimator.SetFloat(SpeedHash, 1f, animatorDampTime, deltaTime);
        AvoidNearbyEnemies(2.5f, 2f, deltaTime);
        FacePlayer();

        //// Move
        //EnemyFinalMove(weightedMove * stateMachine.movementSpeed, deltaTime);
        ////////
        //Vector3 direction = GetChaseDirection();
        //Vector3 avoid = GetObstacleAvoidanceDirection(2f, 2f, stateMachine.obstacleMask);
        //Vector3 separation = GetSeparationForces();
        //Vector3 influence = GetInfluenceSteering(3f, 1.5f); // radius and weight multiplier

        //Vector3 finalMove = (direction + avoid + separation + influence).normalized;
        //EnemyFinalMove(finalMove * stateMachine.movementSpeed, deltaTime);



        //Vector3 toTarget = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).normalized;
        ////if (Time.time >= lastAvoidanceCheckTime + avoidanceRecheckCooldown)
        ////{
        ////    desiredDirection = FindClearDirection(toTarget);
        ////    lastAvoidanceCheckTime = Time.time;
        ////}

        //// Try to avoid obstacles

        //Vector3 desiredDirection = FindClearDirection(toTarget);
        //Vector3 moveDirection = Vector3.Slerp(previousMoveDirection, desiredDirection, directionLerpSpeed * deltaTime);
        //previousMoveDirection = moveDirection;

        //if (moveDirection != Vector3.zero)
        //{
        //    moveDirection = moveDirection.normalized;
        //    // Rotate smoothly toward the move direction
        //    Quaternion targetRot = Quaternion.LookRotation(moveDirection);
        //    stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRot, rotationSpeed * deltaTime);

        //    // Move using CharacterController
        //    //Vector3 movement = moveDirection * stateMachine.movementSpeed * deltaTime;
        //    EnemyFinalMove(moveDirection * stateMachine.movementSpeed, deltaTime);
        //}

        Vector3 toTarget = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).normalized;

        // Avoid obstacles
        if (Time.time >= lastAvoidanceCheckTime + avoidanceRecheckCooldown)
        {
            cachedDesiredDirection = FindClearDirection(toTarget);
            lastAvoidanceCheckTime = Time.time;
        }

        Vector3 moveDirection = Vector3.Slerp(previousMoveDirection, cachedDesiredDirection, directionLerpSpeed * deltaTime);
        previousMoveDirection = moveDirection;

        // Add repulsion
        Vector3 repulsion = GetSeparationVector(); // See below
        float repulsionWeight = 0.5f;  // Lower this to bias toward player
        float obstacleWeight = 1.0f;   // Main pathing direction
        float targetWeight = 2.0f;     // Strong pull toward player



        Vector3 finalDirection =
            (moveDirection * obstacleWeight +
             repulsion * repulsionWeight +
             toTarget * targetWeight).normalized;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDirection);
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRot, rotationSpeed * deltaTime);

            stateMachine.EnemyFinalMove(finalDirection * stateMachine.movementSpeed, deltaTime);
        }


        if (IsInEngagementRange())
        {
            //stateMachine.IsAttackLockout = false;
            CombatPhaseManager.Instance.OnEnemyResumed(stateMachine);

        }

    }

    private Vector3 GetChaseDirection()
    {
        Vector3 toPlayer = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).normalized;

        // Raycast to check for obstacle in front
        if (Physics.Raycast(stateMachine.transform.position + Vector3.up, toPlayer, out RaycastHit hit, 1.5f, stateMachine.obstacleMask))
        {
            // Try sidestep
            Vector3 sidestep = Vector3.Cross(Vector3.up, toPlayer);
            if (!Physics.Raycast(stateMachine.transform.position + Vector3.up, sidestep, 1f, stateMachine.obstacleMask))
            {
                return sidestep;
            }
            else if (!Physics.Raycast(stateMachine.transform.position + Vector3.up, -sidestep, 1f, stateMachine.obstacleMask))
            {
                return -sidestep;
            }
        }

        return toPlayer;
    }
    private void EvaluateNextCombatPhase()
    {


        //switch (stateMachine.combatRole)
        //{
        //    case CombatRoleType.PrimaryAttacker:
        //        {
        //            if (shouldAttackNow)
        //            {
        //                // Director decided you have an attack token right now
        //                SetCombatState(new EnemyAttackingState(this));
        //            }
        //            else
        //            {
        //                // Stay close, ready to be given a token
        //                SetCombatState(new EnemyHoldNearPlayerAttacker(this));
        //            }
        //            break;
        //        }
        //    case CombatRoleType.SecondaryAttacker:
        //        {
        //            if (shouldAttackNow)
        //            {
        //                // Director decided you have an attack token right now
        //                SetCombatState(new EnemyAttackingState(this));
        //            }
        //            else
        //            {
        //                // Stay close, ready to be given a token
        //                SetCombatState(new EnemyHoldNearPlayerState(this));
        //            }
        //            break;
        //        }

        //    case CombatRoleType.Support:
        //    default:
        //        {
        //            // Generic support = circling
        //            SetCombatState(new EnemyCirclingState(this));
        //            break;
        //        }
        //}


    }
    private Vector3 GetSeparationVector()
    {
        Collider[] hits = Physics.OverlapSphere(stateMachine.transform.position, 1.5f, stateMachine.enemyMask);

        Vector3 repulsion = Vector3.zero;
        int count = 0;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyStateMachine other) && other != stateMachine)
            {
                Vector3 dirAway = (stateMachine.transform.position - other.transform.position).normalized;
                repulsion += dirAway;
                count++;
            }
        }

        if (count > 0)
        {
            repulsion /= count;
            repulsion.y = 0;
        }

        return repulsion;
    }
    private Vector3 FindClearDirection(Vector3 baseDir)
    {

        if (!Physics.SphereCast(stateMachine.transform.position, detectionRadius, baseDir, out RaycastHit hit, detectionDistance, stateMachine.obstacleMask))
        {
            lastValidDirection = baseDir;
            return baseDir.normalized;
        }

        float step = maxAvoidanceAngle / angleChecks;

        for (int i = 1; i <= angleChecks; i++)
        {
            float angle = step * i;

            foreach (int sign in new int[] { -1, 1 })
            {
                Vector3 checkDir = Quaternion.Euler(0f, angle * sign, 0f) * baseDir;

                if (!Physics.SphereCast(stateMachine.transform.position, detectionRadius, checkDir, out _, detectionDistance, stateMachine.obstacleMask))
                {
                    lastValidDirection = checkDir;
                    return checkDir.normalized;
                }
            }
        }

        // Fallback: Use last good direction
        return lastValidDirection != Vector3.zero ? lastValidDirection : -baseDir;
    }
    public override void Exit()
    {

        //CombatPhaseManager.Instance.Resume(stateMachine, SuspendReason.Distance);
    }



}
