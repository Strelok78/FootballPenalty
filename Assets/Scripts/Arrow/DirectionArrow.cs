using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    public static DirectionArrow Instance { get; private set; }

    public float rotationSpeed = 60f;
    public float maxAngle = 45f;
    private float currentAngle = 0f;
    private int direction = 1;
    private bool isLocked = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (!gameObject.activeSelf || isLocked) return;

        currentAngle += rotationSpeed * direction * Time.deltaTime;

        if (Mathf.Abs(currentAngle) >= maxAngle)
        {
            direction *= -1;
            currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);
        }

        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
    }

    public void LockDirection()
    {
        isLocked = true;
        Vector3 forward = transform.forward;
        BallController.Instance.SetDirection(forward);
    }

    public void UnlockDirection()
    {
        isLocked = false;
        currentAngle = 0f;
        direction = 1;
    }

    public void ShowArrow()
    {
        gameObject.SetActive(true);
        UnlockDirection();
    }

    public void HideArrow()
    {
        gameObject.SetActive(false);
    }
}