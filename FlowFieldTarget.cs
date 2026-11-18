using UnityEngine;

public class FlowFieldTarget : MonoBehaviour
{
    public static FlowFieldTarget Instance;
    Vector3 lastTargetPosition;
    public float updateThreshold = 1f;
    public float updateCooldown = 0.5f;
    private float timer;
    void Awake()
    {
        Instance = this;
        lastTargetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2Int targetGridPos = GridManager.Instance.GetGridPos(transform.position);
            GridManager.Instance.GenerateFlowField(targetGridPos);
        }


        timer += Time.deltaTime;

        if (Vector3.Distance(transform.position, lastTargetPosition) > updateThreshold && timer > updateCooldown)
        {
            Vector2Int targetGridPos = GridManager.Instance.GetGridPos(transform.position);
            GridManager.Instance.GenerateFlowField(targetGridPos);
            lastTargetPosition = transform.position;
            timer = 0f;
        }
    }
}
