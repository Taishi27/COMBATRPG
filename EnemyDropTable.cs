using UnityEngine;
[CreateAssetMenu(menuName = "Combat/Enemy Drop Table")]
public class EnemyDropTable : ScriptableObject
{
    public EnemyDropEntry[] entries;
    public float spawnScatterRadius = 1f;

    public void RollAndSpawn(Vector3 center)
    {
        if (entries == null || entries.Length == 0)
            return;

        foreach (var entry in entries)
        {
            if (entry == null || entry.prefab == null)
                continue;

            if (Random.value > entry.dropChance)
                continue;

            int count = Random.Range(entry.minCount, entry.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2 circle = Random.insideUnitCircle * spawnScatterRadius;
                Vector3 spawnPos = center + new Vector3(circle.x, 0f, circle.y);

                Object.Instantiate(entry.prefab, spawnPos, Quaternion.identity);
            }
        }
    }
}

