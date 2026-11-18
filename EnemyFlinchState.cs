using UnityEngine;

public class EnemyFlinchState : EnemyBaseState
{

    private readonly int lightFlinchAnimHash = Animator.StringToHash("lightFlinch");



    public float timer;
    public float hitStaggerDamage;

    private AttackAnimationsSO attackData;
    private Vector3 hitDirection;
    public EnemyFlinchState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }
 
    public override void Enter()
    {

        FacePlayer();
        timer = 0f;
        //stateMachine.enemyImpactStateDuration = 0f;
        // Optional: Set state duration per attack


        stateMachine.enemyAnimator.CrossFadeInFixedTime(lightFlinchAnimHash, 0.1f);
        AudioManager.Instance.PlayGrunt(stateMachine.transform.position);
        AudioManager.Instance.PlayEnemyDamageSFX(stateMachine.transform.position);
        stateMachine.IsActiveInCombat = false;


    }
    public override void Tick(float deltaTime)
    {
        timer += deltaTime;
        stateMachine.Move(deltaTime);


        if (timer >= stateMachine.enemyImpactStateDuration)
        {
            //stateMachine.SwitchState(new EnemyReAttackingState(stateMachine));

            //(stateMachine.CurrentState as EnemyReAttackingState)?.EnterHoldNearPlayerPhase();
            //stateMachine.SwitchState(new EnemyReturnToCombatState(stateMachine,stateMachine.PreviousCombatState));
            stateMachine.SwitchState(stateMachine.GetImpactRecoverState());
            //stateMachine.SwitchState(new EnemyAttackingState(stateMachine));
        }
    }
    private void FaceHitDirection()
    {
        Vector3 flatDir = new Vector3(hitDirection.x, 0, hitDirection.z);
        if (flatDir != Vector3.zero)
        {
            stateMachine.transform.rotation = Quaternion.LookRotation(-flatDir); // face attacker
        }
    }
    public override void Exit()
    {

    }
}
