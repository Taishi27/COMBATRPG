using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum StatusEffectType
{
    Fire,
    Ice,
    Poison,
    Lightning
}

public class StatusEffectReceiver : MonoBehaviour
{
    [Header("Dependencies")]
  
    public EnemyStateMachine enemyStateMachine; // or custom enemy movement script
    public Transform statusEffectPosition;
    [Header("Settings")]
    public float fireTickInterval = 1f;
    public float fireDamagePerTick = 10f;
    public float statusDuration = 3.5f;
    public float fireDuration = 2.5f;
    public float lightningDamagePerTick = 5f;
    public float poisonSpeedMultiplier = 0.2f;
    public float iceFreezeTime = 2f;

    private Dictionary<StatusEffectType, Coroutine> activeEffects = new();

    private float originalSpeed;
    private EnemyHealth enemyHealth;


    public event Action<StatusEffectType> OnStatusEffectStarted;
    public event Action<StatusEffectType> OnStatusEffectEnded;


    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();
        if (enemyStateMachine.controller != null)
            originalSpeed = enemyStateMachine.movementSpeed; // adjust if using custom move logic
    }
    public bool IsEffectActive(StatusEffectType type)
    {
        return activeEffects.ContainsKey(type);
    }
    public void ApplyStatusEffect(StatusEffectType type)
    {
        if(enemyStateMachine.enemyHealth.IsDead) return;
        if (activeEffects.ContainsKey(type))
            return;

        Coroutine effectRoutine = StartCoroutine(HandleStatusEffect(type));
        activeEffects[type] = effectRoutine;

        OnStatusEffectStarted?.Invoke(type);
    }

    private IEnumerator HandleStatusEffect(StatusEffectType type)
    {
        // Spawn visual from your global VFX Manager
        enemyStateMachine.StatusEffectsApplied = true;
        //enemyStateMachine.enemyDefenseHandler.enabled = false;
        enemyStateMachine.HandleStunForAbilityEffect();
        string vfxKey = GetVFXKey(type);
        GameObject vfxInstance = GlobalVFXManager.Instance.PlayVFX(
            vfxKey,
            statusEffectPosition.position, // Adjust for visual height
            Quaternion.identity,
            statusDuration + 0.5f
        );
        vfxInstance.transform.SetParent(transform);

        AudioManager.Instance.PlayStatusEffectSFX(type, transform.position);

        switch (type)
        {
            case StatusEffectType.Fire:
                yield return FireDOTRoutine();
                break;

            case StatusEffectType.Ice:
                yield return IceFreezeRoutine();
                break;

            case StatusEffectType.Poison:
                yield return PoisonSlowRoutine();
                break;
            case StatusEffectType.Lightning:
                yield return LightningRoutine();
                break;

        }

        // Cleanup
      
        activeEffects.Remove(type);
        if (vfxInstance != null)
            vfxInstance.transform.SetParent(null);

        enemyStateMachine.StatusEffectsApplied = activeEffects.Count > 0;



        OnStatusEffectEnded?.Invoke(type);
    }





    private IEnumerator FireDOTRoutine()
    {
        float initalVel = enemyStateMachine.enemyAnimator.speed;

        float time = 0f;
        while (time < fireDuration)
        
        {
            enemyStateMachine.enemyAnimator.speed = 0.75f;
            enemyHealth.DealElementalDamage(fireDamagePerTick);
            yield return new WaitForSeconds(fireTickInterval);
            time += fireTickInterval;
        }

        enemyStateMachine.enemyAnimator.speed = initalVel;
    }
    private string GetVFXKey(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Fire => "fire_status",
            StatusEffectType.Ice => "ice_status",
            StatusEffectType.Poison => "poison_status",
            StatusEffectType.Lightning => "lightning_status",
            _ => ""
        };
    }
    private IEnumerator IceFreezeRoutine()
    {
        if (enemyStateMachine.enemyAnimator != null)
            enemyStateMachine.enemyAnimator.speed = 0.3f;

        yield return new WaitForSeconds(iceFreezeTime);

        if (enemyStateMachine.enemyAnimator != null)
            enemyStateMachine.enemyAnimator.speed = 1f;
    }

    private IEnumerator PoisonSlowRoutine()
    {
        if (enemyStateMachine.controller != null)
        {
            enemyStateMachine.movementSpeed *= poisonSpeedMultiplier;
        }
            

        yield return new WaitForSeconds(statusDuration);
        if (enemyStateMachine.controller != null)
        {
            enemyStateMachine.movementSpeed = originalSpeed;
        }
    }
    private IEnumerator LightningRoutine()
    {
        float time = 0f;
        while (time < fireDuration)
        {
            enemyHealth.DealElementalDamage(fireDamagePerTick);
            yield return new WaitForSeconds(fireTickInterval);
            time += fireTickInterval;
        }
    }

}
