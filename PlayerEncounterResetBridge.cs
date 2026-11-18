using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class PlayerEncounterResetBridge : MonoBehaviour
{
    [SerializeField] private playerStateMachine playerStateMachine_;
    [SerializeField] private Mana playerManaHandler;
    [SerializeField] private AbilityManager playerAbilityManager;
    [SerializeField] private Health playerHealth;
    [SerializeField] private CharacterController characterController;
    public Transform fallbackRespawnPoint;

    public void ResetPlayerForEncounter(EncounterManager encounter)
    {
        if (playerStateMachine_ == null)
        {
            playerStateMachine_ = GetComponent<playerStateMachine>();
        }
        
        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }
           
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
         
        if(playerManaHandler == null)
        {
            playerManaHandler = GetComponent<Mana>();
        }

        if(playerAbilityManager == null)
        {
            playerAbilityManager = GetComponent<AbilityManager>();
        }



        Transform respawn = null;

        if (encounter != null)
            encounter.TryGetRespawnPoint(out respawn);

        if (respawn == null)
            respawn = fallbackRespawnPoint;
    

        Debug.Log($"Respawn point = {(respawn != null ? respawn.name : "NULL")}");
        Debug.Log($"Player before reset = {transform.position}");

        if (respawn != null)
        {
            if (characterController != null)
                characterController.enabled = false;

            transform.position = respawn.position;
            transform.rotation = respawn.rotation;

            Debug.Log($"Player after set position = {transform.position}");


            if (characterController != null)
                characterController.enabled = true;
        }

        playerStateMachine_.ResetForEncounterRespawn(respawn);
        //playerStateMachine_.SwitchState(new playerFreeLookState(playerStateMachine_));
        playerManaHandler.ResetManaForRestart();

        ResetPlayerHealth();
       
    }

    private Transform GetEncounterRespawnPoint()
    {
        EncounterManager encounter = RetryContext.PendingEncounterRetry;
        if (encounter != null && encounter.TryGetRespawnPoint(out Transform point))
        {
            return point;
        }

        return fallbackRespawnPoint;
    }

    private void ResetPlayerHealth()
    {
        if (playerHealth == null) return;

        // Hook your actual Health API here
        playerHealth.ResetHealthToFull();
        // playerHealth.ReviveSilently();
    }

    //private void ResetPlayerRuntimeState()
    //{
    //    if (playerStateMachine_ == null) return;

    //    // Hook your actual reset flow here
    //    // clear queued inputs
    //    // clear dodge/block/attack flags
    //    // reset animation state
    //    // restore default locomotion state
    //    // clear force receiver / target lock / camera combat flags
    //}
}
