using UnityEngine;

public class EnemyDeathManager : MonoBehaviour
{
    public static EnemyDeathManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(EnemyDeathContext context)
    {
        Debug.Log(
            $"EnemyDeathManager: Enemy {context.enemyName} died. " +
            $"ID: {context.uniqueEnemyId}, " +
            $"Archetype: {context.archetypeName}, " +
            $"Source: {context.killingBlow.sourceType}, " +
            $"WeaponCategory: {context.killingBlow.weaponCategory}, " +
            $"Location: {context.deathPosition}"
        );

        TrySpawnDrops(context);

        // Future hooks
        // QuestManager.Instance?.NotifyEnemyKilled(context);
        // EncounterManager.Instance?.NotifyEnemyKilled(context);
        AnalyticsManager.Instance?.RecordEnemyKill(context);
        // BestiaryManager.Instance?.RegisterKill(context);
    }

    private void TrySpawnDrops(EnemyDeathContext context)
    {
        if (context.enemyTypeProfile == null)
        {
            Debug.Log("trying to spawn loot but no profile");
            return;
        }
          

        EnemyDropTable dropTable = context.enemyTypeProfile.dropTable;
        if (dropTable == null)
        {
            Debug.Log("trying to spawn loot but no droptable in enemy profile");
            return;
        }
          

        dropTable.RollAndSpawn(context.deathPosition);
        Debug.Log("trying to spawn loot");
    }
}
