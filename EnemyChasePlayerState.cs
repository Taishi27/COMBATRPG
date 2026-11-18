using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class EnemyChasePlayerState : EnemyBaseState
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
    private Vector3 previousMoveDirection = Vector3.forward;
    private float directionLerpSpeed = 2f;

    private float avoidanceRecheckCooldown = 0.2f;
    private float lastAvoidanceCheckTime = 0f;
    private Vector3 cachedDesiredDirection;

    // -------------------------
    // Temporary approach spread
    // -------------------------
    private Vector3 currentApproachPoint;
    private float approachRefreshInterval = 1.25f;
    private float lastApproachRefreshTime;

    private float minApproachRadius = 1.5f;
    private float maxApproachRadius = 3.5f;

    private float minAngleOffset = 20f;
    private float maxAngleOffset = 70f;

    public EnemyChasePlayerState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        CombatCoordinator.Instance.RegisterEnemyIntoCombat(stateMachine);
        stateMachine.IsActiveInCombat = true;

        stateMachine.IsAttackLockout = true;
        stateMachine.enemyAnimator.CrossFadeInFixedTime(LocomotionHash, animatorDampTime);

        Debug.Log("Entering chasing player state");

        RefreshApproachPoint(true);

        Vector3 toTarget = (currentApproachPoint - stateMachine.transform.position).normalized;
        previousMoveDirection = toTarget;
        cachedDesiredDirection = toTarget;
        lastAvoidanceCheckTime = Time.time;
    }

    public override void Tick(float deltaTime)
    {
        stateMachine.enemyAnimator.SetFloat(SpeedHash, 1f, animatorDampTime, deltaTime);

        AvoidNearbyEnemies(5f, 7f, deltaTime);
        FacePlayer();

        if (ShouldRefreshApproachPoint())
        {
            RefreshApproachPoint(false);
        }

        Vector3 toTarget = (currentApproachPoint - stateMachine.transform.position);
        Vector3 toTargetDir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector3.zero;

        if (Time.time >= lastAvoidanceCheckTime + avoidanceRecheckCooldown)
        {
            cachedDesiredDirection = FindClearDirection(toTargetDir);
            lastAvoidanceCheckTime = Time.time;
        }

        Vector3 moveDirection = Vector3.Slerp(previousMoveDirection, cachedDesiredDirection, directionLerpSpeed * deltaTime);
        previousMoveDirection = moveDirection;

        Vector3 finalDirection = (moveDirection + toTargetDir).normalized;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDirection);
            stateMachine.transform.rotation = Quaternion.Slerp(
                stateMachine.transform.rotation,
                targetRot,
                rotationSpeed * deltaTime
            );

            stateMachine.EnemyFinalMove(finalDirection * stateMachine.movementSpeed, deltaTime);
        }

        if (IsInEngagementRange())
        {
            stateMachine.IsAttackLockout = false;

            bool alreadyActive = CombatPhaseManager.Instance.ActiveEnemies.Contains(stateMachine);

            if (!alreadyActive)
            {
                CombatPhaseManager.Instance.TryRegisterEnemy(stateMachine);
            }
            else
            {
                CombatPhaseManager.Instance.OnEnemyResumed(stateMachine);
            }
        }
    }

    private bool ShouldRefreshApproachPoint()
    {
        if (Time.time >= lastApproachRefreshTime + approachRefreshInterval)
            return true;

        float distToApproachPoint = Vector3.Distance(stateMachine.transform.position, currentApproachPoint);
        if (distToApproachPoint < 1.0f)
            return true;

        float distToPlayer = Vector3.Distance(stateMachine.transform.position, stateMachine.playerHealth.transform.position);
        if (distToPlayer < 2.5f)
            return true;

        return false;
    }

    private void RefreshApproachPoint(bool forceImmediate)
    {
        Transform player = stateMachine.playerHealth.transform;
        Vector3 playerPos = player.position;

        Vector3 toEnemy = stateMachine.transform.position - playerPos;
        toEnemy.y = 0f;

        if (toEnemy.sqrMagnitude < 0.01f)
            toEnemy = -player.forward;

        toEnemy.Normalize();

        // Pick a side randomly
        float sign = Random.value < 0.5f ? -1f : 1f;

        // Pick a random angle and radius
        float randomAngle = Random.Range(minAngleOffset, maxAngleOffset) * sign;
        float randomRadius = Random.Range(minApproachRadius, maxApproachRadius);

        Vector3 offsetDir = Quaternion.Euler(0f, randomAngle, 0f) * toEnemy;
        Vector3 candidate = playerPos + offsetDir * randomRadius;

        // Keep Y grounded to enemy current height for CharacterController movement
        candidate.y = stateMachine.transform.position.y;

        currentApproachPoint = candidate;
        lastApproachRefreshTime = Time.time;

        if (forceImmediate)
        {
            cachedDesiredDirection = (currentApproachPoint - stateMachine.transform.position).normalized;
        }
    }

    private Vector3 FindClearDirection(Vector3 baseDir)
    {
        if (baseDir.sqrMagnitude < 0.001f)
            return stateMachine.transform.forward;

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

        return lastValidDirection != Vector3.zero ? lastValidDirection : -baseDir;
    }

    public override void Exit()
    {
    }
}


