using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum HoldNearPlayerStyle
{
    StepBack,
    SidestepLeft,
    SidestepRight,
}
public class EnemyHoldNearPlayerAttacker : EnemyBaseState
{

    private float holdTimer;
    public float holdDuration;
    //private readonly int animHash = Animator.StringToHash("AttackerIdle");
//    private readonly int[] animHashes = new int[]
//{
//    Animator.StringToHash("AttackerIdle"),          // original
//    Animator.StringToHash("AttackerIdle2"),      // slight retreat anim
//    Animator.StringToHash("AttackerIdle3")     // idle fidget or scan
//};
    //private readonly string stepBackAnimHash;
    private readonly int stepBackAnimHash = Animator.StringToHash("AttackerIdle");
    private readonly int sidestepLeftAnimHash = Animator.StringToHash("AttackerIdle2");
    private readonly int sidestepRightAnimHash = Animator.StringToHash("AttackerIdle3");
 
    private Vector3 fallbackTarget;

    private HoldNearPlayerStyle previousStyle = HoldNearPlayerStyle.StepBack; // default
    private HoldNearPlayerStyle currentStyle;

    public EnemyHoldNearPlayerAttacker(EnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        holdDuration = Random.Range(1.5f, 3f);
        holdTimer = 0f;
        stateMachine.IsActiveInCombat = true;
  
        stateMachine.enemyAnimator.applyRootMotion = true;
        stateMachine.enemyAnimator.CrossFadeInFixedTime(stepBackAnimHash, 0.25f);
        

       

  
    }

    public override void Tick(float deltaTime)
    {


 
        stateMachine.Move(deltaTime);
        FacePlayer();
        AvoidNearbyEnemies(4f, 4f, deltaTime);

        holdTimer += deltaTime;
   
        //if (holdTimer >= holdDuration && stateMachine.combatRole == CombatRoleType.PrimaryAttacker || stateMachine.IsInAttackRange() && stateMachine.combatRole == CombatRoleType.PrimaryAttacker)
        //{
        //    stateMachine.SetCombatState(new EnemyAttackingState(stateMachine));
        //}
       if (holdTimer >= holdDuration)
        {
            //CombatPhaseManager.Instance.NotifyAttackerReady(stateMachine);
            holdTimer = 0f; // prevent spamming if director doesn't react immediately
        }
        //else if (!stateMachine.IsInAttackRange())
        //{
        //    stateMachine.SwitchState(new EnemyReEngageForwardState(stateMachine));
        //}
    }
    public override void HandleRootMotion(Vector3 rootMotionDelta, float deltaTime)
    {
        stateMachine.EnemyFinalMove(rootMotionDelta / deltaTime, deltaTime);
    }
    private HoldNearPlayerStyle ChooseStyleSmart()
    {
        float distanceToPlayer = Vector3.Distance(stateMachine.transform.position, stateMachine.playerHealth.transform.position);

        List<HoldNearPlayerStyle> weightedOptions = new List<HoldNearPlayerStyle>();

        // Weight logic: 75% StepBack (if not last used), 25% Sidestep
        if (previousStyle != HoldNearPlayerStyle.StepBack)
            weightedOptions.AddRange(Enumerable.Repeat(HoldNearPlayerStyle.StepBack, 3));
        else
            weightedOptions.AddRange(Enumerable.Repeat(HoldNearPlayerStyle.StepBack, 1)); // minimal repeat weight

        if (previousStyle != HoldNearPlayerStyle.SidestepLeft)
            weightedOptions.Add(HoldNearPlayerStyle.SidestepLeft);
        if (previousStyle != HoldNearPlayerStyle.SidestepRight)
            weightedOptions.Add(HoldNearPlayerStyle.SidestepRight);

        // Optionally avoid sidesteps if too far from player (tactical)
        if (distanceToPlayer > 5f)
        {
            weightedOptions.Remove(HoldNearPlayerStyle.SidestepLeft);
            weightedOptions.Remove(HoldNearPlayerStyle.SidestepRight);
        }

        if (weightedOptions.Count == 0)
            return HoldNearPlayerStyle.StepBack; // Fallback if all removed

        int randomIndex = Random.Range(0, weightedOptions.Count);
        return weightedOptions[randomIndex];
    }
    public override void Exit()
    {
        Debug.Log("Exiting Holding near player attacker");
        stateMachine.enemyAnimator.applyRootMotion = false;
    }
}

