
using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public float animatorDampTime = 0.1f;


    public int DeathAnimnHash = Animator.StringToHash("Death");

    private float deathDuration;
    private float elapsedTime;
    private AttackAnimationsSO attackData;
    private EnemyDeathContext deathContext;

    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }
    public void SetDeathAnimation(AttackAnimationsSO attackData)
    {
        this.attackData = attackData;
    
        // Optional: assign animation from attack data
        if (!string.IsNullOrEmpty(attackData.enemyDeathAnimationName))
            DeathAnimnHash = Animator.StringToHash(attackData.enemyDeathAnimationName);

        // Optional: vary death duration
       
    }
    public override void Enter()
    {
        deathContext = stateMachine.LastDeathContext;
        stateMachine.isPaused = false;
        if (stateMachine.controller != null)
        {
            stateMachine.controller.enabled = false;
        }

        AudioManager.Instance.PlayEnemyDedSFX(stateMachine.transform.position);
        CombatPhaseManager.Instance.TryUnregisterEnemy(stateMachine);
        CombatCoordinator.Instance.DeRegisterEnemyFromCombat(stateMachine);
 
        stateMachine.enemyAnimator.CrossFadeInFixedTime(DeathAnimnHash, animatorDampTime);
        stateMachine.enemyWeapon.gameObject.SetActive(false);
        stateMachine.enemyHealth.healthSlider.gameObject.SetActive(false) ;
        Debug.Log(stateMachine.name + "is ded now ");

      
        deathDuration = 3f;
        elapsedTime = 0f;

       


    }
    public override void Tick(float deltaTime)
    {
        elapsedTime += deltaTime;

        if (elapsedTime >= deathDuration)
        {
            if (stateMachine.CombatBodyCollider != null) stateMachine.CombatBodyCollider.enabled = false;
            var allColliders = stateMachine.GetComponentsInChildren<Collider>();
            foreach (var col in allColliders) col.enabled = false;
            Object.Destroy(stateMachine.gameObject);
        }
       
    }
    public override void Exit()
    {
        Debug.Log("idk why but i am exiting this state" + stateMachine.name);
    }

   
}
