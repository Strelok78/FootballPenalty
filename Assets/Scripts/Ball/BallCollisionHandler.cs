using System;
using UnityEngine;
using UnityEngine.Events;

public class BallCollisionHandler : MonoBehaviour
{
    [NonSerialized] public GameManager gameManager;
    
    public float resetDelay = 0.5f;
    public UnityEvent onGoalScored;

    void Start()
    {
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            onGoalScored?.Invoke();
            Invoke(nameof(ResetGame), resetDelay);
        }
    }

    private void ResetGame()
    {
        if (gameManager != null)
        {
            gameManager.ResetGame();

            BallMovement ballMovement = GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.StopMovement();
                ballMovement.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogError("BallCollisionHandler: BallMovement component not found on the ball!");
            }
        }
        else
        {
            Debug.LogError("BallCollisionHandler: GameManager not assigned!");
        }
    }
}