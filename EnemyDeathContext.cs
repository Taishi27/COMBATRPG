using UnityEngine;
[System.Serializable]
public struct EnemyDeathContext
{ 
    public string uniqueEnemyId;
    public string enemyName;

    public Vector3 deathPosition;
    public Vector3 deathForward;
    public float timeOfDeath;

    public EnemyTypeProfile enemyTypeProfile; // your SO / profile
    public string archetypeName;              // easy to read fallback
    public int level;                         // optional if applicable

    public DamageSourceInfo killingBlow;

    public EnemyStateMachine enemy;
    public GameObject enemyGameObject;
}
