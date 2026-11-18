using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyEffectsManager : MonoBehaviour
{
   
    public EnemyStateMachine StateMachine;
    public float lifeTime = 1.0f;

    public Transform attack1Point, attack2Point, attack3Point, SpecialAttackPoint;
    public string AttackVFXName;
    // Start is called before the first frame update
    private void Awake()
    {
        StateMachine = GetComponent<EnemyStateMachine>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowBloodSplatterFX(Vector3 BloodSplatterLocation, Vector3 hitDirection)
    {
        Quaternion rotation = Quaternion.LookRotation(hitDirection);

    

        GlobalVFXManager.Instance.PlayVFX("BloodSplatter",BloodSplatterLocation, rotation);
    }

    public void SpawnBlockSparks(Vector3 hitPos)
    {
        GlobalVFXManager.Instance.PlayVFX("BlockClash", hitPos, Quaternion.identity);
     
    }

    public void SpawnParryClash(Vector3 hitPos)
    {
        GlobalVFXManager.Instance.PlayVFX("ParryClash", hitPos, Quaternion.identity,0.7f);
     
    }
    public void SpawnPerfectParryClash(Vector3 hitPos)
    {
        GlobalVFXManager.Instance.PlayVFX("PerfectParryClash", hitPos, Quaternion.identity, 0.7f);

    }
    public void SpawnSwordVFX()
    {
     
    }

    public void CheckParryFromAnimation()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (var col in hits)
        {
            var parry = col.GetComponentInParent<ParryStateHandler>();
            if (parry != null)
            {
                //parry.RegisterHitMoment();
            }
        }
    }
    public void SpawnSwordVFXEnemy(string key)
    {
      
        Transform point = GetSpawnPoint(key);
        if (key != null)
        {
            GlobalVFXManager.Instance.PlayVFX(key, point.position, point.rotation);

        }
    }

    public void SpawnEnemyAttackVFX()
    {

        if(AttackVFXName != null)
        {
            GlobalVFXManager.Instance.PlayVFX(AttackVFXName, SpecialAttackPoint.position, SpecialAttackPoint.rotation,4f);
        }
    }
    public Transform GetSpawnPoint(string key)
    {
        if (key == null) return null;
        // Use keywords in your key to decide the point
        if (key.Contains("ATK1Slash")) return attack1Point;
        if (key.Contains("ATK2Slash")) return attack2Point;
        if (key.Contains("ATK3Slash")) return attack3Point;

        return transform; // fallback to root
    }
}
