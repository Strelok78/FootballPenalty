using UnityEngine;
using System.Collections;

public class BallCollisionHandler : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    public PlayerController playerController;
    public float resetDelay = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        if (playerController == null)
        {
            Debug.LogError("Не назначена ссылка на PlayerController в BallCollisionHandler!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            StartCoroutine(ResetAfterDelay());
        }
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetBall();
        ResetPlayer();
    }

    void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    private void ResetPlayer()
    {
        if (playerController != null && playerController.initialPlayerPosition != null)
        {
            playerController.transform.position = playerController.initialPlayerPosition.position;
            playerController.canKick = true;
            if (playerController.ball != null)
            {
                AimArrowController arrowController = playerController.ball.GetComponent<AimArrowController>();
                if (arrowController != null)
                {
                    arrowController.ShowArrow();
                    arrowController.SetIsAiming(true);
                }
                else if (playerController.ball.transform.parent != null && playerController.ball.transform.parent.Find("ArrowPivot") != null)
                {
                    GameObject arrowPivotObject = playerController.ball.transform.parent.Find("ArrowPivot").gameObject;
                    if (arrowPivotObject != null)
                    {
                        arrowPivotObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Не установлена initialPlayerPosition или не назначена ссылка на PlayerController.");
        }
    }
}