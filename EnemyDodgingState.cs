using UnityEngine;

public class EnemyDodgingState : EnemyBaseState
{
    private readonly int DodgeLeftHash = Animator.StringToHash("DodgeLeft");
    private readonly int DodgeRightHash = Animator.StringToHash("DodgeRight");
    private Vector3 dodgeDirection;
    private float duration; // Duration of the Dodge
    private float timer;

    private int chosenDirection = 0;
    public EnemyDodgingState(EnemyStateMachine stateMachine,float timer) : base(stateMachine)
    {
        duration = timer;
    }

    public override void Enter()
    {
    
        timer = 0f;

        // Determine dodge direction relative to player
        Vector3 toPlayer = stateMachine.playerHealth.transform.position - stateMachine.transform.position;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer.normalized);
        chosenDirection = Random.value > 0.5f ? 1 : -1;

        dodgeDirection = right * chosenDirection;

        int animHash = chosenDirection > 0 ? DodgeRightHash : DodgeLeftHash;

        stateMachine.enemyAnimator.CrossFadeInFixedTime(animHash, 0.1f);
        stateMachine.enemyHealth.isInvulnerable = true;

    }

  

    public override void Tick(float deltaTime)
    {
        timer += deltaTime;

        Vector3 move = dodgeDirection.normalized * stateMachine.movementSpeed;
        stateMachine.EnemyFinalMove(move, deltaTime);

        if (timer >= duration)
        {
          
            stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));

        }
        FacePlayer();
    }

    public override void Exit()
    {
        stateMachine.enemyHealth.isInvulnerable = false;
        //stateMachine.enemyDefenseHandler.StartCoroutine(stateMachine.enemyDefenseHandler.ResetEvaluation());
        stateMachine.enemyDefenseHandler.ResetEvaluation();
    }
}
