
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;
    public bool isPLayerInRange = false;
    public Vector3 finalMovementVector;

    private Collider[] obstacleBuffer = new Collider[32]; // Cached buffer
    public EnemyBaseState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }


    protected bool IsInChaseRange()
    {
        if (stateMachine.playerHealth.IsDead) { return false; }
        float DistToPlayer = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).sqrMagnitude;

        return DistToPlayer <= stateMachine.playerDetectionRange * stateMachine.playerDetectionRange;

    }
    public bool IsPlayerInVisionCone()
    {
        if (stateMachine.playerHealth == null || stateMachine.playerHealth.IsDead)
            return false;

        Vector3 toPlayer = stateMachine.playerHealth.transform.position - stateMachine.transform.position;
        float distance = toPlayer.magnitude;

        if (distance > stateMachine.visionRadius)
            return false;

        // Angle check
        float angleToPlayer = Vector3.Angle(stateMachine.transform.forward, toPlayer.normalized);
        if (angleToPlayer > stateMachine.visionAngle / 2f)
            return false;




        return true;
    }

    //protected void Move(float deltatime)
    //{
    //    if(stateMachine.controller != null && stateMachine.controller.enabled)
    //    {
    //        EnemyFinalMove(Vector3.zero + stateMachine.forceReciever.movement, deltatime);
    //    }
        
    //}

    //protected void EnemyFinalMove(Vector3 motion, float deltaime)
    //{
    //    if (stateMachine.controller != null && stateMachine.controller.enabled)
    //    {
    //        //stateMachine.combatPushback.UpdatePushDirection();

    //        //motion += stateMachine.combatPushback.GetPushVector();
    //        // Gravity applied here regardless of state
    //        if (!stateMachine.IsGrounded())
    //        {
    //            stateMachine.forceReciever.verticalVelocity += stateMachine.forceReciever.GravityVal* deltaime;
    //        }
    //        else if (stateMachine.forceReciever.verticalVelocity < 0f)
    //        {
    //            stateMachine.forceReciever.verticalVelocity = -2f;
    //        }

    //        Vector3 finalMotion = motion;
    //        finalMotion += Vector3.up * stateMachine.forceReciever.verticalVelocity;

    //        finalMotion += stateMachine.forceReciever.movement;

    //        //stateMachine.controller.Move(finalMotion * deltaime);
    //        finalMotion += ApplySmartSteering(finalMotion, deltaime);

    //        stateMachine.controller.Move((finalMotion + stateMachine.forceReciever.movement) * deltaime);
    //    }




    //}

    protected void FacePlayer()
    {
        if (stateMachine == null) { return; }
        Vector3 lookPos = stateMachine.playerHealth.transform.position - stateMachine.transform.position;
        lookPos.y = 0f;

        stateMachine.transform.rotation = Quaternion.LookRotation(lookPos);
    }

    protected bool IsInEngagementRange()
    {
        if (stateMachine.playerHealth.IsDead) { return false; }
        float playerDistSqr = (stateMachine.playerHealth.transform.position - stateMachine.transform.position).sqrMagnitude;
        return playerDistSqr <= stateMachine.EngagementRange * stateMachine.EngagementRange;
    }

    public float IsInDisengagementRange()
    {
        return Vector3.Distance(stateMachine.transform.position, stateMachine.playerHealth.transform.position);
    }

    protected Vector3 TryAdjustForObstacles(Vector3 moveDir)
    {
        Vector3 origin = stateMachine.transform.position + Vector3.up * 0.4f;
        Vector3 finalMoveDir = moveDir;

        RaycastHit hit;
        if (Physics.SphereCast(origin, 3f, moveDir, out hit, 2f, stateMachine.obstacleMask))
        {
            // Steer away from obstacle based on surface normal
            Vector3 avoidDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
            finalMoveDir = Vector3.Lerp(moveDir, avoidDir, 0.8f); // smooth blend
        }

        return finalMoveDir.normalized;
    }
    protected Vector3 ApplySmartSteering(Vector3 desiredMove, float deltaTime)
    {
        Vector3 adjustedDir = TryAdjustForObstacles(desiredMove);
        Vector3 separation = GetSeparationForce() * 2f; // Tweak multiplier

        Vector3 finalMove = (adjustedDir + separation).normalized;

        return finalMove;
    }




    protected Vector3 GetSeparationForce()
    {
        Vector3 separation = Vector3.zero;
        obstacleBuffer = Physics.OverlapSphere(stateMachine.transform.position, 4f);

        foreach (var hit in obstacleBuffer)
        {
            EnemyStateMachine otherEnemy = hit.transform.GetComponentInParent<EnemyStateMachine>();
            if (otherEnemy == null || otherEnemy == stateMachine)
                continue;


            if (hit.CompareTag("EnemyCollider"))
            {

                Vector3 toSelf = stateMachine.transform.position - hit.transform.position;
                float distance = toSelf.magnitude;

                // Only apply force if within close proximity (e.g., < 1.2f)
                if (distance < 1.2f)
                {
                    float strength = 5f; // Adjust this based on feel
                    separation += toSelf.normalized * strength;

                    Debug.Log($"[SEPARATE] {stateMachine.name} from {hit.name} | Distance: {distance} | Strength: {strength}");
                }
            }
        }

        return separation;
    }


    public void OnDrawGizmosSelected()
    {
        // Draw facing direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(stateMachine.transform.position, stateMachine.transform.position + stateMachine.transform.forward * 1.5f);

        // Draw detection radius
        Gizmos.color = new Color(1, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(stateMachine.transform.position, stateMachine.playerDetectionRange);

        // Draw current slot (if any)
        //if (stateMachine.slotIndex != -1 && stateMachine.claimedSlotPos.HasValue)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(stateMachine.claimedSlotPos.Value, 0.25f);
        //    Gizmos.DrawLine(stateMachine.transform.position, stateMachine.claimedSlotPos.Value);
        //}


    }

    protected void AvoidNearbyEnemies(float separationDistance = 1.5f, float repulsionStrength = 1.5f, float deltaTime = 0.02f)
    {
        Collider[] hits = Physics.OverlapSphere(stateMachine.transform.position, separationDistance, stateMachine.enemyMask);

        Vector3 repulsion = Vector3.zero;
        int repulsedCount = 0;
        if (repulsedCount == 0) return;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyStateMachine other) && other != stateMachine)
            {
                Vector3 dirAway = (stateMachine.transform.position - other.transform.position).normalized;
                repulsion += dirAway;
                repulsedCount++;
            }
        }

        if (repulsedCount > 0)
        {
            repulsion /= repulsedCount;
            repulsion.y = 0;
            Vector3 repulsionMove = repulsion * repulsionStrength;
            Debug.Log("repulsed");
            stateMachine.EnemyFinalMove(repulsionMove, deltaTime);
        }
    }
}
