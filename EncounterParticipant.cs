using UnityEngine;

public class EncounterParticipant : MonoBehaviour ,IEncounterResettable
{
    [Header("Ownership")]
    [SerializeField] private EncounterManager ownerEncounter;

    [Header("Rules")]
    [SerializeField] private bool countsTowardCompletion = true;
    [SerializeField] private bool disableObjectOnDeath = false;
    [SerializeField] private bool resetTransformOnReset = true;
    [SerializeField] private bool despawnIfSpawned = true;
    [Header("Optional References")]
    public EnemyHealth health;
    [SerializeField] private EncounterActivationMode initialMode = EncounterActivationMode.Passive;
    [SerializeField] private EnemyStateMachine enemyStateMachineParent;
    [SerializeField] private MonoBehaviour enemyStateMachineBehaviour; // optional, if you want to disable/re-enable
    [SerializeField] private GameObject rootToToggle;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool initialActiveState;
    private bool deathReported;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool savedRootActive;
    private bool wasSpawnedAtRuntime;
    //private bool defeatReported;


    public EncounterManager OwnerEncounter => ownerEncounter;
    public bool CountsTowardCompletion => countsTowardCompletion;
    public bool IsDeadReported => deathReported;

    public EncounterActivationMode InitialMode => initialMode;

    private void Awake()
    {
        if (rootToToggle == null)
            rootToToggle = gameObject;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialActiveState = rootToToggle.activeSelf;

        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }

        enemyStateMachineParent = GetComponent<EnemyStateMachine>();
        
    }

    public void AssignEncounter(EncounterManager encounter, bool runtimeSpawned = false)
    {
        ownerEncounter = encounter;
        wasSpawnedAtRuntime = runtimeSpawned;
    }
    public void SaveEncounterState()
    {
        savedPosition = transform.position;
        savedRotation = transform.rotation;
        savedRootActive = rootToToggle != null && rootToToggle.activeSelf;
        deathReported = false;
    }
    public void ActivateForEncounter()
    {
        if (health.IsDead) return;
        deathReported = false;

        //if (rootToToggle != null && !rootToToggle.activeSelf)
        //    rootToToggle.SetActive(true);

        //if (enemyStateMachineBehaviour != null)
        //    enemyStateMachineBehaviour.enabled = true;

        enemyStateMachineParent.SetCombatState(new EnemySignalState(enemyStateMachineParent));
    }

    public void DeactivateForEncounter()
    {
        if (enemyStateMachineBehaviour != null)
            enemyStateMachineBehaviour.enabled = false;
    }

    public void NotifyDeath()
    {
        if (deathReported)
            return;

        deathReported = true;
        ownerEncounter?.NotifyParticipantDefeated(this);

        if (disableObjectOnDeath && rootToToggle != null)
            rootToToggle.SetActive(false);
    }

    public void ResetForEncounter()
    {
        deathReported = false;

        if (rootToToggle != null)
            rootToToggle.SetActive(initialActiveState);


        if (wasSpawnedAtRuntime && despawnIfSpawned)
        {
            Destroy(gameObject);
            return;
        }


        transform.position = savedPosition;
        transform.rotation = savedRotation;

        enemyStateMachineParent.ResetForEncounter();

        ApplyActivationMode(initialMode);

        // Hook your own health reset here.
        // Example:
        health?.ResetHealthToFull();

        // Hook your own enemy AI reset here if needed.
        // Example:
        // var enemy = GetComponent<EnemyStateMachine>();
        // enemy?.ResetToEncounterStartState();
    }
    private void SetScriptedMode()
    {
        if (enemyStateMachineParent != null)
        {
            enemyStateMachineParent.enabled = true;
            // TODO:
            // enemyStateMachine.SetEncounterActivationMode(EncounterActivationMode.Scripted);
            // enemyStateMachine.EnterScriptedState();
        }
    }

    private void SetPassiveMode()
    {
        if (enemyStateMachineParent != null)
        {
            enemyStateMachineParent.enabled = true;
            // TODO:
            // enemyStateMachine.SetEncounterActivationMode(EncounterActivationMode.Passive);
            // enemyStateMachine.EnterPassiveState();
        }
    }

    private void SetCombatMode()
    {
        if (enemyStateMachineParent != null)
        {
            // TODO:
            // enemyStateMachine.SetEncounterActivationMode(EncounterActivationMode.Combat);
            // enemyStateMachine.EnterCombatReadyState();
            enemyStateMachineParent.SetCombatState(new EnemySignalState(enemyStateMachineParent));
        }
    }
    private void ResetEnemyRuntimeState()
    {
        if (enemyStateMachineParent == null) return;

        // TODO: plug into your real AI reset API:
        // enemyStateMachine.ResetForEncounter();
        // CombatCoordinator.Instance?.Unregister(enemyStateMachine);
        // CombatPhaseManager.Instance?.Unregister(enemyStateMachine);
    }
    public void ApplyActivationMode(EncounterActivationMode mode)
    {
        switch (mode)
        {
            case EncounterActivationMode.Scripted:
                SetScriptedMode();
                break;

            case EncounterActivationMode.Passive:
                SetPassiveMode();
                break;

            case EncounterActivationMode.Combat:
                SetCombatMode();
                break;
        }
    }

    public void ResetEncounterState()
    {
        
    }
}
