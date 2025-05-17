using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BallMovement: Rigidbody component missing on the Ball!");
            enabled = false;
            return;
        }
        rb.useGravity = false;
    }

    public void SetTarget(Vector3 target, float kickForce)
    {
        rb.linearVelocity = Vector3.zero;
        Vector3 direction = (target - transform.position).normalized;
        rb.AddForce(direction * kickForce, ForceMode.Impulse);
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
    }
}