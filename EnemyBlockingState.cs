using UnityEngine;

public class EnemyBlockingState : EnemyBaseState
{
    private readonly int BlockHash = Animator.StringToHash("Block");

    private float timer;

    private float minBlockDuration;
    private float maxBlockDuration;
    private float noPressureExitDelay;

    private float lastBlockedHitTime;
    private int blockedHits;

    private bool pendingResponse;

    public EnemyBlockingState(EnemyStateMachine stateMachine, float duration) : base(stateMachine)
    {
        minBlockDuration = duration;
        maxBlockDuration = duration + 1.0f;          // safety cap
        noPressureExitDelay = 0.35f;                 // leave block if player stops pressuring
    }

    public override void Enter()
    {
        stateMachine.enemyAnimator.applyRootMotion = true;
        //stateMachine.IsBlocking = true;
        if (stateMachine.ParryStateHandler != null)
        {
            stateMachine.ParryStateHandler.SetBlocking(true);

            if (stateMachine.CurrentEnemyType.archetype == EnemyType.Brute)
            {
                stateMachine.ParryStateHandler.AttemptParry();
            }
        }

        timer = 0f;
        blockedHits = 0;
        lastBlockedHitTime = Time.time;
        pendingResponse = false;

   

        stateMachine.enemyAnimator.CrossFadeInFixedTime(BlockHash, 0.1f);

        // lock new reactive defense for a brief moment so we don't spam-switch
        stateMachine.enemyDefenseHandler.defenseLockedUntilTime = Time.time + 0.5f;
    }

    public override void Tick(float deltaTime)
    {
        timer += deltaTime;

        stateMachine.Move(deltaTime);

        // if we have blocked enough hits, do something smarter than just standing there
        if (!pendingResponse && blockedHits >= 2)
        {
            pendingResponse = true;
            TryBranchResponse();
            return;
        }

        // after minimum block time, if player pressure stopped, leave block
        if (timer >= minBlockDuration)
        {
            if (Time.time - lastBlockedHitTime > noPressureExitDelay)
            {
                ExitToCombat();
                return;
            }
        }

        // hard cap
        if (timer >= maxBlockDuration)
        {
            ExitToCombat();
            return;
        }
    }

    public void OnBlockedHit(AttackAnimationsSO attackData, Vector3 hitDirection)
    {
        blockedHits++;
        lastBlockedHitTime = Time.time;
        Debug.Log("added a blocked hit");
        // slight lock refresh so spam doesn't instantly farm a free opening
        stateMachine.enemyDefenseHandler.defenseLockedUntilTime = Time.time + 0.35f;
        if (blockedHits >= 3)
        {
            stateMachine.ParryStateHandler?.AttemptParry();
        }
        // optional: tiny block recoil anim could be played here
        // optional: perfect block / clash fx here
    }

    private void TryBranchResponse()
    {
        // Choose a smarter next step after sustained pressure
        // grunt = often dodge or shove away
        // brute = maybe immediate counter/parry
        // basic fallback = exit into shuffle/chase
        if (stateMachine.enemyDefenseHandler != null && stateMachine.enemyDefenseHandler.CanDefend())
        {
            if (Random.value < 0.5f)
            {
                stateMachine.EnterDodgeState();
                return;
            }
        }

        ExitToCombat();
    }

    private void ExitToCombat()
    {
        

        stateMachine.SetCombatState(new EnemyShuffleState(stateMachine));
    }

    public override void Exit()
    {
        stateMachine.enemyAnimator.applyRootMotion = false;
        //stateMachine.IsBlocking = false;
        stateMachine.ParryStateHandler?.ResetParry();
        stateMachine.enemyDefenseHandler.ResetEvaluation();
        stateMachine.enemyDefenseHandler.nextDefenseAllowedTime = Time.time + 0.75f;
    }
}
