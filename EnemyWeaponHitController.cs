using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitController : MonoBehaviour
{
    [SerializeField]
    private List<WeaponHitSource> hitSources = new();

    public void EnableHitSource(string id)
    {
        var source = hitSources.Find(s => s.id == id);
        if (source == null) return;

        source.active = true;
        source.WeaponLogic.SetActive(true);
    
   
    }

    public void DisableHitSource(string id)
    {
        var source = hitSources.Find(s => s.id == id);
        if (source == null) return;

        source.active = false;
        source.WeaponLogic.SetActive(false);

    }

    public void DisableAll()
    {
        foreach (var s in hitSources)
        {
            s.active = false;
            s.collider.enabled = false;
        }
    }
}
