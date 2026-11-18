
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class EnemyPatrollingState : EnemyBaseState
{

 


    public EnemyPatrollingState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.patrolSpline == null) return;
        stateMachine.waitTimer = 0;
        stateMachine.enemyAnimator.CrossFadeInFixedTime("Locomotion", 0.1f);
        stateMachine.waitingAtEnd = false;
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.IsPlayerInVisionCone())
        {
            stateMachine.SwitchState(new EnemySignalState(stateMachine));
        }
        //stateMachine.waitDuration-= deltaTime;

        //if (stateMachine.patrolGridPoints == null || stateMachine.patrolGridPoints.Count == 0) return;

        //Vector2Int currentGridPos = GridManager.Instance.GetGridPos(stateMachine.transform.position);
        //Vector2Int targetGrid = stateMachine.patrolGridPoints[stateMachine.patrolIndex];

        //Vector2 flowDir = GridManager.Instance.GetDirectionTo(currentGridPos, targetGrid);
        //if (flowDir == Vector2.zero) return;

        //Vector3 moveDir = new Vector3(flowDir.x, 0, flowDir.y);
        //Vector3 finalMove = ApplySmartSteering(moveDir, deltaTime);

        //EnemyFinalMove(finalMove * stateMachine.patrollingSpeed, deltaTime);
        //FaceDirection(finalMove);
        //stateMachine.enemyAnimator.SetFloat("EnemySpeed", 0.5f);

        //Vector3 targetWorld = GridManager.Instance.GetWorldPosition(targetGrid);

        //if (Vector3.Distance(stateMachine.transform.position, targetWorld) < minDist)
        //{
        //    stateMachine.patrolIndex = (stateMachine.patrolIndex + 1) % stateMachine.patrolGridPoints.Count;
        //}


        //if (IsPlayerInVisionCone())
        //{
        //   stateMachine.SwitchState(new EnemySignalState(stateMachine));
        //}

        if (stateMachine.waitingAtEnd)
        {
            stateMachine.waitTimer += deltaTime;
            if (stateMachine.waitTimer >= stateMachine.patrolDelay)
            {
                stateMachine.waitTimer = 0f;
                stateMachine.waitingAtEnd = false;
                stateMachine.splineT = 0f; // restart from start
            }
            return;
        }

        if (stateMachine.patrolSpline == null)
            return;

        // Move along spline
        stateMachine.splineT += (stateMachine.splineMoveSpeed * deltaTime) / stateMachine.splineLength;
        stateMachine.splineT = Mathf.Clamp01(stateMachine.splineT);

        //stateMachine.patrolSpline.Evaluate(stateMachine.splineT, out var pos, out var tangent);
        float3 pos;
        float3 tangent;
        float3 up;

        // Evaluate the spline
        stateMachine.patrolSpline.Evaluate(stateMachine.splineT, out pos, out tangent, out up);

        // Convert float3 to Vector3
        Vector3 targetPos = (Vector3)pos;

        // Move along spline
        Vector3 moveDir = (targetPos - stateMachine.transform.position).normalized;
        Vector3 move = moveDir * stateMachine.patrollingSpeed;

        Vector3 adjusted = TryAdjustForObstacles(moveDir); // Your existing logic
        Vector3 finalMove = adjusted * stateMachine.patrollingSpeed;

        stateMachine.EnemyFinalMove(finalMove, deltaTime);
        FaceDirection(finalMove);

        // Detect end of spline
        if (Vector3.Distance(stateMachine.transform.position, targetPos) < 0.4f && stateMachine.splineT >= 0.99f)
        {
            stateMachine.waitingAtEnd = true;
            stateMachine.enemyAnimator.SetFloat("EnemySpeed", 0f);
        }
        else
        {
            stateMachine.enemyAnimator.SetFloat("EnemySpeed", 0.5f);
        }

     



    }

    private void FaceDirection(Vector3 direction)
    {
        Vector3 flatTangent = new Vector3(direction.x, 0f, direction.z).normalized;
        if (flatTangent.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatTangent);
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    public override void Exit() { }
}

