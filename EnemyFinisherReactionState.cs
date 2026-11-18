using UnityEngine;

public class EnemyFinisherReactionState : EnemyBaseState
{
    private Transform player;
    private float animDuration;
    private float timer;
  

    public EnemyFinisherReactionState(EnemyStateMachine stateMachine, playerStateMachine playerState) : base(stateMachine)
    {
        player = playerState.transform;
    }

    public override void Enter()
    {
        Vector3 toEnemy = (stateMachine.transform.position - player.transform.position).normalized;
        toEnemy.y = 0f;
        animDuration = stateMachine.enemyAnimator.GetCurrentAnimatorStateInfo(0).length;
        timer = 0f;
        // Position the player a fixed distance from the enemy, facing them
        float finisherOffset = 1.25f; // tweak this based on your animation
        Vector3 desiredPlayerPos = stateMachine.transform.position - toEnemy * finisherOffset;

        player.transform.position = desiredPlayerPos;
        player.transform.rotation = Quaternion.LookRotation(toEnemy);

        // Rotate enemy to face player (optional but useful for synced finishers)
        stateMachine.transform.rotation = Quaternion.LookRotation(-toEnemy);

        stateMachine.StartCoroutine(stateMachine.HandleEnemyFinishedAnimation());
        stateMachine.enemyFinishableUI.SetActive(false);

    }

    public override void Exit()
    {
     
        stateMachine.IsActiveInCombat = false;
        var allColliders = stateMachine.GetComponentsInChildren<Collider>();
        foreach (var col in allColliders) col.enabled = false;
    }

    public override void Tick(float deltaTime)
    {
        stateMachine.controller.enabled = false;
        timer += deltaTime;

        if (timer >= 2f)
        {
            stateMachine.SwitchState(new EnemyPostFinisherState(stateMachine));

        }
    }
}
