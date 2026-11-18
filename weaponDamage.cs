
using System.Collections.Generic;


using UnityEngine;
public enum CombatResult
{
    PerfectParry,
    NormalParry,
    Block,
    Hit,
    Dead,
    None // fallback if nothing applied
}
public enum DamageSourceType
{
    Unknown = 0,
    Weapon = 1,
    Magic = 2,
    Projectile = 3,
    Environmental = 4,
    Ultimate = 5,
    StatusEffect = 6
}
public enum WeaponCategory
{
    None = 0,
    Light = 1,
    Medium = 2,
    Heavy = 3
}
public class weaponDamage : MonoBehaviour
{

    [SerializeField] private Collider myCollider;
    public float wepDamageAmount;
    public float weaponKnockbackAmount;
    private List<Collider> alreadyCollidedWith = new List<Collider>();

    public Transform weaponTip;
    public TrailRenderer trail;
    public float trailTime = 0.2f;
    private void OnEnable()
    {
        alreadyCollidedWith.Clear();
    }

   
    private void Awake()
    {
        trail = weaponTip.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.emitting = false;  // disable at start
            trail.time = trailTime;
        }

      
    }
    private void OnTriggerEnter(Collider other)
    {
        // --- Stage 0: Validation
        if (other == myCollider) return;
        if (!other.CompareTag("PlayerCombatCollider")) return;
        if (alreadyCollidedWith.Contains(other)) return;

        alreadyCollidedWith.Add(other);

        // --- Stage 1: Conflict resolution (Attacker = Enemy, Defender = Player)
        var attackerCombatant = GetComponentInParent<ICombatant>();             // Enemy
        var defenderCombatant = other.GetComponentInParent<ICombatant>();       // Player

        if (attackerCombatant != null && defenderCombatant != null)
        {
            var outcome = ConflictResolver.Resolve(attackerCombatant, defenderCombatant);

            if (outcome == ConflictOutcome.DefenderWins)
            {
                // Player wins simultaneous trade  cancel our damage
                attackerCombatant.OnConflictResolvedAsLoser(defenderCombatant);
                defenderCombatant.OnConflictResolvedAsWinner(attackerCombatant);

                // cleanup this contact so we can detect future hits properly
                alreadyCollidedWith.Remove(other);
                return;
            }

            if (outcome == ConflictOutcome.Clash)
            {
                // Cinematic clash, no damage
                attackerCombatant.OnConflictClash(defenderCombatant);
                defenderCombatant.OnConflictClash(attackerCombatant);

                alreadyCollidedWith.Remove(other);
                return;
            }

            // AttackerWins  continue to your parry/block/damage flow
        }

        // --- Stage 2: Gather info (unchanged)
        Vector3 hitPos = weaponTip != null ? weaponTip.position : transform.position;
        Vector3 dir = (other.transform.position - hitPos).normalized;

        var parry = other.GetComponentInParent<ParryStateHandler>();
        var health = other.GetComponentInParent<Health>();
        var effects = other.GetComponentInParent<EffectsManager>();
        var force = other.GetComponentInParent<forceRecieverPlayer>();
        var playerSM = other.GetComponentInParent<playerStateMachine>();

        // --- Stage 3: Resolve your existing parry/block outcome
        CombatResult result = ResolveCombatOutcome(parry);

        // --- Stage 4: Apply effects & damage (your logic preserved)
        HandleOutcome(result, playerSM, health, effects, force, hitPos, dir);

    }
    private CombatResult ResolveCombatOutcome(ParryStateHandler parry)
    {
        if (parry == null) return CombatResult.Hit;

        if (parry.IsParryWindowActive)
        {
            parry.RegisterHitMoment();

            if (parry.IsPerfectParry)
                return CombatResult.PerfectParry;
            else
                return CombatResult.NormalParry;
        }

        if (parry.IsBlocking)
            return CombatResult.Block;

        return CombatResult.Hit;
    }

    private void HandleOutcome(CombatResult result, playerStateMachine playerSM,Health health,EffectsManager effects,forceRecieverPlayer force,Vector3 hitPos,Vector3 dir)
    {
        var EEM = GetComponentInParent<EnemyEffectsManager>();
        //player did this so no recoil
        switch (result)
        {
            case CombatResult.PerfectParry:
                AudioManager.Instance.PlayParryClash(Camera.main.transform.position);
                EEM.SpawnPerfectParryClash(GlobalVFXManager.Instance.parryBlockVFXpoint.position);
                Time.timeScale = 0.1f;
                Invoke(nameof(ResetTime), 0.03f);
                UIPromptManager.I.Show("Player HIT Perfect Parry!!", 1.2f, PromptChannel.Interact);
                Debug.Log("Perfect Parry!");
                GetComponentInParent<EnemyStateMachine>()?.HandleStun();
                AbilityManager.Instance.AddChargeFromSwordHit();
                //playerSM?.HandleBlockedRecoil(1f);
                playerSM?.TriggerParryFOV();
                AbilityCameraManager.Instance.TriggerFlash();
                AbilityCameraManager.Instance.triggerShockWave();
                break;

            case CombatResult.Block:
                AudioManager.Instance.PlayBlockSound(Camera.main.transform.position);
                EEM.SpawnBlockSparks(GlobalVFXManager.Instance.parryBlockVFXpoint.position);
                //playerSM?.HandleBlockedRecoil(0.25f);
                playerSM?.TriggerCameraShake(0.35f);

                if (health != null)
                {
                    float reducedDamage = wepDamageAmount * 0.3f;
                    health.DealDamage(reducedDamage);
                }
                break;

            case CombatResult.Hit:
                if (health != null && !health.isInvulnerable)
                {
                    ///player got hit //
                    CombatPhaseManager.Instance.OnPlayerTookDamage(EEM.StateMachine);
                    health.DealDamage(wepDamageAmount);
                    effects?.ShowBloodSplatterFX(hitPos, dir);
                }
                playerSM?.TriggerCameraShake(0.5f);
                break;
        }

        // Knockback always applies on hit, block, parry recoil
        if (force != null)
        {
            Vector3 direction = (playerSM.transform.position - myCollider.transform.position).normalized;
            force.AddForce(direction * weaponKnockbackAmount);
        }
    }

    public void SetAttack(float damageAmt, float knockBack)
    {
        wepDamageAmount = damageAmt;
        weaponKnockbackAmount = knockBack;

    }
    private void ResetTime()
    {
        Time.timeScale = 1f;
    }
    public void EnableTrail()
    {
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    // Called from animation event
    public void DisableTrail()
    {
        if (trail != null)
        {
            trail.emitting = false;
        }
    }
}
