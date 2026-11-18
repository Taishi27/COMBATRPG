using System;
using System.Collections;
using Michsky.UI.Heat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider; // Drag the Slider here in Inspector
    [SerializeField] private ProgressBar myBar;
    public ParryStateHandler parryHandler;
    private float health;

    public float originalHealth;
    public event Action OnTakeDamage;
    public event Action OnDie;

    public bool IsDead => health <= 0;
    public float MaxHealth => maxHealth;
    public float CurrentHealth;
    public bool isInvulnerable = false;

    [Header("Damage Volume")]
    public Material DamageShader;
    public float DamageVignetteIntensity = 5.7f;
    public float DamageVignettePower;
    public playerHealthBarUIBinder barUI;
    public float DamageVignetteConstThreshold;
    private Coroutine healRoutine;
    public playerStatusEffectReciever statusEffectReciever;
   
    private void Awake()
    {
        health = maxHealth;
        UpdateHealthBar();
        parryHandler = GetComponent<ParryStateHandler>();
        DamageShader.SetFloat("_vignettePower", 6.51f);
        DamageShader.SetFloat("_vignetteIntensity", 0f);
        DamageVignetteConstThreshold = maxHealth * 0.4f;
        statusEffectReciever = GetComponent<playerStatusEffectReciever>();
        originalHealth = health;
      
        if (barUI != null && statusEffectReciever != null)
        {
            barUI.Bind(statusEffectReciever);
        }
    }
    private void Update()
    {

        CurrentHealth = health;
        if (health <= DamageVignetteConstThreshold)
        {
            DamageVignetteHandler(2, 5f);
        }

       
    }
    public void DealDamage(float damageAmount)
    {
        if (IsDead) return;
        if (isInvulnerable) return;
        if (parryHandler != null && parryHandler.IsBlocking)
        {
            float reduced = damageAmount * 0.3f;
            health = Mathf.Clamp(health - reduced, 0f, maxHealth);
            StartCoroutine(DamageVignetteHandler(1.2f,2f));
            UpdateHealthBar();
            return;
        }

        health = Mathf.Clamp(health - damageAmount, 0f, maxHealth);
        OnTakeDamage?.Invoke();
        UpdateHealthBar();



        StartCoroutine(DamageVignetteHandler(1.2f,2f));

        if(health <= (0.4 * MaxHealth))
        {
            UIPromptManager.I.Show("Low Health � find cover or heal!!", 2f, PromptChannel.Bottom);
        }



        if (health <= 0f)
        {
            OnDie?.Invoke();
        }
    }
    public void DealElementalDamage(float damageAmount)
    {
        if (IsDead) return;
        if (isInvulnerable) return;
        if (parryHandler != null && parryHandler.IsBlocking)
        {
            float reduced = damageAmount * 0.3f;
            health = Mathf.Clamp(health - reduced, 0f, maxHealth);
            StartCoroutine(DamageVignetteHandler(1.2f, 2f));
            UpdateHealthBar();
            return;
        }

        health = Mathf.Clamp(health - damageAmount, 0f, maxHealth);
        //OnTakeDamage?.Invoke();
        UpdateHealthBar();



        StartCoroutine(DamageVignetteHandler(1.2f, 2f));

        if (health <= (0.4 * MaxHealth))
        {
            UIPromptManager.I.Show("Low Health � find cover or heal!!", 2f, PromptChannel.Bottom);
        }



        if (health <= 0f)
        {
            OnDie?.Invoke();
        }
    }
    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        if(myBar != null)
        {
            myBar.maxValue = maxHealth;
            myBar.SetValue(health);
            myBar.UpdateUI();
        }

     

    }

    public void SetHealthBar(Slider slider)
    {
        healthSlider = slider;
        UpdateHealthBar();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        UpdateHealthBar();
    }
    public void HealOverTime(float totalAmount, float duration)
    {
        if (healRoutine != null)
            StopCoroutine(healRoutine);

        healRoutine = StartCoroutine(HealOverTimeRoutine(totalAmount, duration));
    }

    public IEnumerator DamageVignetteHandler(float LerpIntensity,float timer)
    {
        float intensity = DamageShader.GetFloat("_vignetteIntensity");


        float LerpedVignette = Mathf.Lerp(intensity, DamageVignetteIntensity, LerpIntensity);

        DamageShader.SetFloat("_vignetteIntensity", LerpedVignette);

        yield return new WaitForSeconds(timer);

        float intensity2 = DamageShader.GetFloat("_vignetteIntensity");

        float ResetVignette = Mathf.Lerp(intensity2, 0f, 1f);


        DamageShader.SetFloat("_vignetteIntensity", ResetVignette);
    }

    private void OnDisable()
    {
        DamageShader.SetFloat("_vignetteIntensity", 0f);
    }

    private IEnumerator HealOverTimeRoutine(float totalAmount, float duration)
    {
        float elapsed = 0f;

        // how much to heal per second
        float rate = totalAmount / duration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // heal for this frame
            float add = rate * Time.deltaTime;

            // apply additively to CURRENT health (so instant heals stay)
            health = Mathf.Min(health + add, maxHealth);
            UpdateHealthBar();

            yield return null;
        }

        // don't snap to an old target here!
        healRoutine = null;
    }
    // --- Optional: Tick-based variant (heals every X seconds) ---
    public void HealOverTime_Ticks(float totalAmount, float duration, int ticks = 10)
    {
        if (healRoutine != null)
            StopCoroutine(healRoutine);

        healRoutine = StartCoroutine(HealOverTimeTicksRoutine(totalAmount, duration, ticks));
    }
    public void ResetHealthToFull()
    {
        Heal(originalHealth);
    }
    private IEnumerator HealOverTimeTicksRoutine(float totalAmount, float duration, int ticks)
    {
        float tickDelay = duration / ticks;
        float healPerTick = totalAmount / ticks;

        for (int i = 0; i < ticks; i++)
        {
            health = Mathf.Clamp(health + healPerTick, 0f, maxHealth);
            UpdateHealthBar();
            yield return new WaitForSeconds(tickDelay);
        }

        healRoutine = null;
    }
}
