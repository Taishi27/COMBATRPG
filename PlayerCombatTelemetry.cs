using UnityEngine;

public class PlayerCombatTelemetry : MonoBehaviour
{
    public static PlayerCombatTelemetry Instance { get; private set; }
    [Header("Tuning")]
    [SerializeField] float hitGain = 0.15f;
    [SerializeField] float damageTakenGain = 0.30f;
    [SerializeField] float decayPerSecond = 0.25f;

    public float Value01 { get; private set; }  // 0–1

    void Awake() => Instance = this;

    void Update()
    {
        if (Value01 <= 0f) return;
        Value01 = Mathf.Max(0f, Value01 - decayPerSecond * Time.deltaTime);
    }

    public void OnPlayerHitEnemy()
    {
        Value01 = Mathf.Clamp01(Value01 + hitGain);
    }

    public void OnPlayerTookDamage()
    {
        Value01 = Mathf.Clamp01(Value01 + damageTakenGain);
    }
}
