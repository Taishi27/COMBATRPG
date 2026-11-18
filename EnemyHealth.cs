using System;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public Slider healthSlider; // Drag the Slider here in Inspector
    public ParryStateHandler parryHandler;
    private float health;

    public event Action<AttackAnimationsSO, Vector3,float> OnTakeDamage;
    public event Action<AttackAnimationsSO> OnDie;
    public EnemyStateMachine enemyStateMachine;
    public StatusEffectReceiver statusEffectReceiver;
    public bool IsDead;
    public float MaxHealth => maxHealth;
    public float CurrentHealth;
    public bool isInvulnerable = false;

    public float consecutiveHits = 0;
    public float hitResetTime = 2f;
    private float lastHitTime;
    public int currentHitThreshold;
    public int hitThreshold;
    public AttackAnimationsSO defaultAttackData;
    [SerializeField] private AttackAnimationsSO deathAnimData;

    [SerializeField] private StaggerHandler staggerHandler;


    public static event Action<EnemyDeathContext> OnAnyEnemyDied;
    //[SerializeField] private EnemyTypeProfile enemyTypeProfile;


    private DamageSourceInfo lastDamageSource;
    private void Awake()
    {
        health = maxHealth;
        UpdateHealthBar();
        parryHandler = GetComponent<ParryStateHandler>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();
        staggerHandler = GetComponent<StaggerHandler>();
        statusEffectReceiver = GetComponent<StatusEffectReceiver>();
        hitThreshold = UnityEngine.Random.Range(1, 4);
        currentHitThreshold = hitThreshold;
        EnemyHealthBarUI barUI = GetComponentInChildren<EnemyHealthBarUI>();
        if (barUI != null && statusEffectReceiver != null)
        {
            barUI.Bind(statusEffectReceiver);
        }
    }
    private void Update()
    {
        CurrentHealth = health;
        //if (CurrentHealth <= enemyStateMachine.finisherThreshold)
        //{
        //    enemyStateMachine.enemyFinishableUI.SetActive(true);
        //}

    }

    public void DealDamage(float damageAmount, float staggerDamage, AttackAnimationsSO attackData, Vector3 hitDirection, DamageSourceInfo damageSourceInfo)
    {
        if (IsDead) return;
        if (isInvulnerable) return;

        lastDamageSource = damageSourceInfo;

        //if (enemyStateMachine.IsBlocking)
        //{
        //    EnemyBlockingState blockingState = enemyStateMachine.CurrentState as EnemyBlockingState;
        //    if (blockingState != null)
        //    {
        //        blockingState.OnBlockedHit(attackData, hitDirection);
        //    }

        //    // optional chip damage
        //    float chipDamage = damageAmount * 0.05f;
        //    health = Mathf.Clamp(health - chipDamage, 0f, maxHealth);
        //    UpdateHealthBar();

        //    // fire block VFX/SFX here
        //    return;
        //}

        lastHitTime = Time.time;
        consecutiveHits++;

        float finalDamage = damageAmount;

        

        health = Mathf.Clamp(health - finalDamage, 0f, maxHealth);

        OnTakeDamage?.Invoke(attackData, hitDirection, staggerDamage);
        UpdateHealthBar();

        if (health <= 0f)
        {
            //IsDead = true;
            HandleDeath();
            enemyStateMachine.Target.isDead = true;
            OnDie?.Invoke(attackData);
            return;
        }

        if (consecutiveHits >= currentHitThreshold)
        {
            consecutiveHits = UnityEngine.Random.Range(1f, 4f);

            if (enemyStateMachine.enemyDefenseHandler.CanDefend())
            {
                TryDodgeOnOverwhelm();
                hitThreshold = UnityEngine.Random.Range(1, 4);
                currentHitThreshold = hitThreshold;
            }
        }

    }
    public void DealDefaultDamage(float damageAmount, DamageSourceInfo damageSourceInfo)
    {
        // Use default values for attackData and hitDirection
        DealDamage(damageAmount,50f, defaultAttackData, Vector3.zero,damageSourceInfo);


    }

    public void DealElementalDamage(float damageAmount)
    {
        if (IsDead) return;
        if (isInvulnerable) return;

        lastHitTime = Time.time;
        consecutiveHits++;
        health = Mathf.Clamp(health - damageAmount, 0f, maxHealth);
        DamageNumberManager.Instance.SpawnDamageNumberPopupEnemy(damageAmount, statusEffectReceiver.statusEffectPosition);


        UpdateHealthBar();
        if (parryHandler != null && parryHandler.IsBlocking)
        {
            float reduced = damageAmount * 0.3f;
            health = Mathf.Clamp(health - reduced, 0f, maxHealth);
            DamageNumberManager.Instance.SpawnDamageNumberPopupEnemy(reduced, statusEffectReceiver.statusEffectPosition);
            UpdateHealthBar();
            return;

        }


        if (health <= 0f)
        {
            HandleDeath();
            enemyStateMachine.Target.isDead = true;
            OnDie?.Invoke(defaultAttackData);
        }
    }
    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    public void SetHealthBar(Slider slider)
    {
        healthSlider = slider;
        UpdateHealthBar();
    }
    public void SetHealthBarUI(EnemyHealthBarUI barUI)
    {
        if (barUI == null) return;

        healthSlider = barUI.HealthSlider;
        UpdateHealthBar();

        if (statusEffectReceiver != null)
        {
            barUI.Bind(statusEffectReceiver);
        }
    }
    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        UpdateHealthBar();
        GlobalVFXManager.Instance.PlayVFX("HealingFX", transform.position, Quaternion.identity);
    }
    private EnemyDeathContext BuildDeathContext()
    {
        EnemyDeathContext ctx = new EnemyDeathContext
        {
            uniqueEnemyId = enemyStateMachine != null ? enemyStateMachine.UniqueEnemyId : gameObject.GetInstanceID().ToString(),
            enemyName = gameObject.name,
            deathPosition = transform.position,
            deathForward = transform.forward,
            timeOfDeath = Time.time,
            enemyTypeProfile = enemyStateMachine.CurrentEnemyType,
            archetypeName = enemyStateMachine.CurrentEnemyType != null ? enemyStateMachine.CurrentEnemyType.name : "Unknown",
            level = 0,
            killingBlow = lastDamageSource,
            enemy = enemyStateMachine,
            enemyGameObject = gameObject
        };

        return ctx;
    }

    private void HandleDeath()
    {
        if (IsDead) return;
        IsDead = true;

        EnemyDeathContext deathContext = BuildDeathContext();

        // Fire global event first
        OnAnyEnemyDied?.Invoke(deathContext);
        GetComponent<EncounterParticipant>()?.NotifyDeath();
        // Push info into state machine / dead state if needed
        if (enemyStateMachine != null)
        {
            enemyStateMachine.SetDeathContext(deathContext);
            enemyStateMachine.SwitchState(new EnemyDeadState(enemyStateMachine));
        }
    }
    private void TryDodgeOnOverwhelm()
    {
        if (enemyStateMachine.enemyDefenseHandler != null)
        {
            enemyStateMachine.enemyDefenseHandler.TryEvaluateDefense();

        }
        //enemyStateMachine.SwitchState(new EnemyBlockingState(enemyStateMachine,0.4f));
    
    }

    public void ResetHealthToFull()
    {
        Heal(maxHealth);
   
        hitThreshold = UnityEngine.Random.Range(1, 4);
        currentHitThreshold = hitThreshold;
        EnemyHealthBarUI barUI = GetComponentInChildren<EnemyHealthBarUI>();
        if (barUI != null && statusEffectReceiver != null)
        {
            barUI.Bind(statusEffectReceiver);
        }
    }
}
