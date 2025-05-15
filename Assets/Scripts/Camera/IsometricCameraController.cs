using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    public Transform ballTransform;
    public Transform playerQueueStart;
    public float offsetY = 10f;
    public float offsetZ = -10f;
    public float minZ = -10f;
    public float maxZ = -30f;
    public int playersVisibleLimit = 5;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (ballTransform == null || playerQueueStart == null) return;

        // Подсчёт расстояния между первым и последним игроком
        int queueSize = playerQueueStart.childCount;
        float dynamicZOffset = offsetZ;

        if (queueSize > playersVisibleLimit)
        {
            dynamicZOffset = offsetZ - (queueSize - playersVisibleLimit) * 1.5f;
            dynamicZOffset = Mathf.Clamp(dynamicZOffset, maxZ, minZ);
        }

        Vector3 newPos = ballTransform.position + new Vector3(0, offsetY, dynamicZOffset);
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5f);
        transform.rotation = Quaternion.Euler(45, 0, 0);
    }
}