using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController Instance { get; private set; }

    private Rigidbody rb;
    private Vector3 kickDirection;
    public float kickForce = 10f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetDirection(Vector3 direction)
    {
        kickDirection = direction.normalized;
    }

    public void Kick()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
    }
}