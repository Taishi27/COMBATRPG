using UnityEngine;


public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }
    
    public float animatorDampTime = 0.1f;

    public float animatorDampTime2 = 0.1f;
    public readonly int LocomotionHash = Animator.StringToHash("Locomotion");

    public readonly int SpeedHash = Animator.StringToHash("EnemySpeed");

    private float idleTimer = 0f;
    private float idleDuration;

    public override void Enter()
    {
        idleTimer = 0f;
        if(stateMachine.enemyAnimator != null)
        {
            stateMachine.enemyAnimator.CrossFadeInFixedTime(LocomotionHash, animatorDampTime);
        }
      
    }
    public override void Tick(float deltaTime)
    {
        stateMachine.Move(deltaTime);

        idleTimer += deltaTime;

        if (idleTimer >= stateMachine.IdleWaitDuration)
        {
            stateMachine.SwitchState(new EnemyPatrollingState(stateMachine));
        }




        if (IsInChaseRange())
        {

            if (!EnemyAlertSystem.playerHasBeenSpotted)
            {
                EnemyAlertSystem.playerHasBeenSpotted = true;
                stateMachine.SwitchState(new EnemySignalState(stateMachine));
                return;// War cry / flute state
            }
            else
            {
                CombatPhaseManager.Instance.TryRegisterEnemy(stateMachine);
                //stateMachine.enemyAnimator.SetFloat(SpeedHash, 0f, animatorDampTime2, deltaTime);
                Debug.Log("in range");
            }

          

        }

    
        //else
        //{
        //    stateMachine.SwitchState(new EnemyChasingState(stateMachine)); // Normal alert reaction
        //}
        //else if(stateMachine.playerSpotted)
        //{
        //    stateMachine.SwitchState(new EnemyChasingState(stateMachine));
        //}
       
    }
    public override void Exit()
    {
        
    }

  
}
