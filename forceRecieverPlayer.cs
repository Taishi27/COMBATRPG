using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class forceRecieverPlayer : MonoBehaviour
{

    [Header("Refs")]
    [SerializeField] private CharacterController playerCC;

    [Header("Gravity")]
    [SerializeField] private float gravity = -30f;        // world gravity (m/s^2)
    [SerializeField] private float groundedGravity = -2f; // small stick-to-ground

    [Header("Impulses")]
    [SerializeField] private float impulseDampHalfLife = 0.25f; // seconds to halve strength

    private Vector3 externalHorizontalVelocity; // knockbacks/dashes (XZ)
    private float verticalVelocity;           // gravity/jumps (Y)

    // Optional helper: directional bias for next impulse
    private Vector3? directionalBias;

    // Exposed to your state code (you already add this to motion in FinalMovement)
    public Vector3 movement => externalHorizontalVelocity + Vector3.up * verticalVelocity;

    void Reset()
    {
        playerCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- Gravity ---
        if (playerCC.isGrounded && verticalVelocity <= 0f)
        {
            // keep the controller grounded without pinning to 0
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // --- Damp external horizontal impulses exponentially (frame-rate independent) ---
        if (externalHorizontalVelocity.sqrMagnitude > 0.0001f)
        {
            // decay factor so velocity halves every 'impulseDampHalfLife' seconds
            float k = 1f - Mathf.Exp(Mathf.Log(0.5f) * Time.deltaTime / Mathf.Max(0.0001f, impulseDampHalfLife));
            externalHorizontalVelocity = Vector3.Lerp(externalHorizontalVelocity, Vector3.zero, k);

            // clamp tiny values to zero
            if (externalHorizontalVelocity.sqrMagnitude < 0.0004f)
                externalHorizontalVelocity = Vector3.zero;
        }
    }

    // Horizontal impulse (knockback/dash). Supply world-space direction.
    public void AddForce(Vector3 impulse)
    {
        impulse.y = 0f;
        externalHorizontalVelocity += impulse;
    }

    // Pure vertical impulse (jump, launchers)
    public void AddVerticalImpulse(float vImpulse)
    {
        verticalVelocity = Mathf.Max(verticalVelocity, 0f); // don't fight upward motion
        verticalVelocity += vImpulse;
    }

    // Optional helpers you already had
    public void SetDirectionalBias(Vector3 dir) => directionalBias = dir.normalized;

    public Vector3 GetAndClearDirectionalBiasOrForward(Transform fallback)
    {
        Vector3 result = directionalBias.HasValue ? directionalBias.Value : fallback.forward;
        directionalBias = null;
        return result;
    }

    public void ResetForces()
    {
        // Clear all accumulated force / vertical velocity / external impulses
        externalHorizontalVelocity = Vector3.zero;
        //movement = Vector3.zero;
        verticalVelocity = 0f;
    }
}
