using UnityEngine;

public class EnemyEdgeRepositionState : EnemyBaseState
{
    private float repositionTimer = 0f;
    private float maxRepositionTime = 1.5f;

    private float rotationSpeed = 200f;
    private Vector3 safeDirection;
    private bool foundSafeDirection = false;

    public EnemyEdgeRepositionState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {

        Debug.Log("repo near edge");
        stateMachine.enemyAnimator.CrossFadeInFixedTime("Idle", 0.1f); // Optional animation
        foundSafeDirection = FindSafeDirection();
    }

    public override void Tick(float deltaTime)
    {
        repositionTimer += deltaTime;

        if (foundSafeDirection)
        {
            // Smoothly rotate toward the safe direction
            Quaternion targetRotation = Quaternion.LookRotation(safeDirection);
            stateMachine.transform.rotation = Quaternion.RotateTowards(
                stateMachine.transform.rotation,
                targetRotation,
                rotationSpeed * deltaTime
            );
        }

        // Timeout or facing safe direction, resume normal AI
        if (repositionTimer >= maxRepositionTime || IsFacingSafeDirection())
        {
            EvaluateNextCombatPhase(); 
        }
    }

    public override void Exit()
    {
        repositionTimer = 0f;
    }

    // Attempts to find a safer direction to turn toward
    private bool FindSafeDirection()
    {
        Vector3[] directionsToTry = {
            -stateMachine.transform.forward,
            stateMachine.transform.right,
            -stateMachine.transform.right
        };

        foreach (Vector3 dir in directionsToTry)
        {
            if (IsGroundAhead(dir))
            {
                safeDirection = dir;
                return true;
            }
        }

        // Default to back off if no direction is safe
        safeDirection = -stateMachine.transform.forward;
        return false;
    }

    private bool IsGroundAhead(Vector3 direction)
    {
        // Manually offset a test position
        Vector3 testOrigin = stateMachine.edgeDetector.raycastOrigin.position + direction.normalized * stateMachine.edgeDetector.rayOffset;
        return Physics.Raycast(testOrigin, Vector3.down, stateMachine.edgeDetector.rayDistance, stateMachine.edgeDetector.groundMask);
    }

    private bool IsFacingSafeDirection()
    {
        Vector3 flatForward = stateMachine.transform.forward;
        flatForward.y = 0;
        return Vector3.Dot(flatForward.normalized, safeDirection.normalized) > 0.95f;
    }

    private void EvaluateNextCombatPhase()
    {
        if (!IsInEngagementRange())
        {
            stateMachine.SwitchState(new EnemyChasePlayerState(stateMachine));
            return;
        }

        switch (stateMachine.combatRole)
        {
            case CombatRoleType.PrimaryAttacker:
                if (stateMachine.IsInAttackRange())
                    stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
                else
                    stateMachine.SetCombatState(new EnemyReEngageForwardState(stateMachine));
                break;

            case CombatRoleType.SecondaryAttacker:
                stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));
                break;

            case CombatRoleType.Support:
                stateMachine.SetCombatState(new EnemyCirclingState(stateMachine));
                break;
        }
    }
}

