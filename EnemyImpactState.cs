using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{
    public int ImpactHash = Animator.StringToHash("Impact");
    private readonly int lightFlinchAnimHash = Animator.StringToHash("lightFlinch");


   
    public float timer;
    public float hitStaggerDamage;
    public float Duration;
    private AttackAnimationsSO attackData;
    private Vector3 hitDirection;
    public EnemyImpactState(EnemyStateMachine stateMachine,float duration) : base(stateMachine)
    {
        Duration = duration;
    }
    public void SetImpactReaction(AttackAnimationsSO data, Vector3 direction)
    {
        attackData = data;
        hitDirection = direction;


        //if (data == null)
        //{
        //    Debug.LogError("Impact reaction called with NULL attack data");
        //    return;
        //}

        // Optional: Set animation hash dynamically
        if (!string.IsNullOrEmpty(data.enemyImpactAnimationName))
        {
            ImpactHash = Animator.StringToHash(data.enemyImpactAnimationName);
        }


    }
    public override void Enter()
    {
        Debug.Log("enemy OOF");
        //FacePlayer();
        timer = 0f;
        //stateMachine.enemyImpactStateDuration = 0f;
        // Optional: Set state duration per attack
        stateMachine.enemyAnimator.CrossFadeInFixedTime(ImpactHash, 0.1f);
        stateMachine.weaponHandler.DisableWeapon();
        stateMachine.IsAttackLockout = true;
        AudioManager.Instance.PlayGrunt(stateMachine.transform.position);
        AudioManager.Instance.PlayEnemyDamageSFX(stateMachine.transform.position);
        stateMachine.IsActiveInCombat = false;
        if (stateMachine.ShieldHandler != null)
        {
            stateMachine.ShieldHandler.SetShieldActive(false);
            Debug.Log("SHIELD ON");
        }

    }
    public override void Tick(float deltaTime)
    {
        timer += deltaTime;
        stateMachine.Move(deltaTime);

       
        if(timer >= Duration) 
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
