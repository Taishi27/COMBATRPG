using UnityEngine;

[System.Serializable]
public class EnemyDropEntry
{
    public GameObject prefab;
    [Range(0f, 1f)] public float dropChance = 1f;
    public int minCount = 1;
    public int maxCount = 1;
}
