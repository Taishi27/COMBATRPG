using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class FinisherManager : MonoBehaviour
{
    public static FinisherManager Instance { get; private set; }

    [Header("Settings")]

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private CinemachineCamera finisherCam;
    public playerStateMachine playerStateMachine;
    public AttackAnimationsSO skill1;
    [SerializeField] private CinemachineTargetGroup finisherTargetGroup;

    [Header("Post-Processing")]
    public Volume abilityVolume;
    [SerializeField] private VolumeProfile abilityProfile;


    [SerializeField] private float volumeTransitionDuration = 0.5f;
    private Coroutine volumeBlendRoutine;

    public Transform finisherCameraTarget;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
       
    }

    public void TryExecuteFinisher()
    {

        //Collider[] hits = Physics.OverlapSphere(transform.position, finisherRadius, enemyLayer);

        //foreach (var hit in hits)
        //{
        //    var enemy = hit.GetComponentInParent<EnemyStateMachine>();
        //    if (enemy == null)
        //    {
        //        Debug.LogWarning("No EnemyStateMachine on " + hit.name);
        //        continue;
        //    }

        //    if (!enemy.IsFinishable)
        //    {
        //        Debug.Log("Enemy found but not finishable: " + enemy.name);
        //        continue;
        //    }

        //    else
        //    {
        //        Debug.Log("Finisher triggered on " + enemy.name);
        //        SetCamPos(enemy.transform);
        //    }



        //    return;
        //}

      
       StartCoroutine(ExecuteFinisherCinematicSequence(1f, 3f));
        
      
    }
    public IEnumerator ExecuteFinisherCinematicSequence(float freezeTime, float pauseDuration)
    {
        // Step 1: Setup
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnterAbilityVolumeEffect();  // Fade screen / overlay
        //CombatCoordinator.Instance.TriggerEnemyPause(pauseDuration);  // Pause everyone except finisher targets
        AbilityCameraManager.Instance.CamFOVZoomIn();
        playerStateMachine.HandleFinisherState(skill1);
        // Step 2: Initial Slow-Mo (for dramatic entrance)
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(freezeTime);
      
        // Step 3: Reset to normal time for clean execution
        Time.timeScale = 1f;

        // Optional delay for animation windup / camera blend
        yield return new WaitForSeconds(0.5f); // Replace with animation duration if needed

        // Step 4: Trigger actual finisher logic (camera/VFX handled elsewhere)
        // e.g., play animation, damage enemy, sync positions etc.
        // FinisherManager.TriggerFinisher(player, enemy);

        // Step 5: Wait to let VFX/Cam finish
        yield return new WaitForSeconds(1f); // Match this with camera duration

        // Step 6: Cleanup
        ExitAbilityVolumeEffect();
        AbilityCameraManager.Instance.CamFOVOriginal();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void PlayCinematicCam(Transform lookAt, Transform follow)
    {
        if(finisherCam != null)
        {
         
            finisherCam.gameObject.SetActive(true);
            finisherCam.LookAt = lookAt;
          
            finisherCam.Follow = follow;
            finisherCam.Priority = 20;
        }
    
    }
    public void EnterAbilityVolumeEffect()
    {
        if (abilityVolume == null || abilityProfile == null) return;

        abilityVolume.profile = abilityProfile;

        if (volumeBlendRoutine != null) StopCoroutine(volumeBlendRoutine);
        volumeBlendRoutine = StartCoroutine(BlendAbilityVolume(1f, volumeTransitionDuration));
    }

    public void ExitAbilityVolumeEffect()
    {
        if (abilityVolume == null) return;

        if (volumeBlendRoutine != null) StopCoroutine(volumeBlendRoutine);
        volumeBlendRoutine = StartCoroutine(BlendAbilityVolume(0f, 3f));
    }
    public IEnumerator BlendAbilityVolume(float targetWeight, float duration)
    {
        float t = 0f;
        float initialWeight = abilityVolume.weight;

        while (t < duration)
        {
            t += Time.deltaTime;
            abilityVolume.weight = Mathf.Lerp(initialWeight, targetWeight, t / duration);
            yield return null;
        }

        abilityVolume.weight = targetWeight;
    }
    public void RestoreDefaultCamera()
    {
        finisherCam.gameObject.SetActive(false);
        finisherCam.Priority = 0;
    }

    public void AddEnemyToTargetGroup(Transform enemy)
    {
        finisherTargetGroup.AddMember(enemy, 1f, 2f);
    }
    public void RemovenemyFromargetGroup(Transform enemy)
    {
        finisherTargetGroup.RemoveMember(enemy);
    }

    public void SetCamPos(Transform enemy)
    {
        Vector3 midpoint = (transform.position + enemy.position) * 0.5f;
        finisherCameraTarget.position = midpoint;
        Vector3 lookDir = (enemy.position - transform.position).normalized;
        finisherCameraTarget.rotation = Quaternion.LookRotation(lookDir);
    }
}