using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform targetPosition;
    private bool isMoving = false;
    private bool inputEnabled = false;

    private PlayersQueue queueManager;

    public void SetQueueManager(PlayersQueue manager)
    {
        queueManager = manager;
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

    private void OnMouseDown()
    {
        Debug.Log("Button clicked!");
        if (!inputEnabled || isMoving) return;

        inputEnabled = false;
        DirectionArrow.Instance.ShowArrow();
        StartCoroutine(MoveAndKick());
    }

    private IEnumerator MoveAndKick()
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPosition.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        DirectionArrow.Instance.LockDirection();
        yield return new WaitForSeconds(0.2f);

        BallController.Instance.Kick();

        yield return new WaitForSeconds(0.5f);
        DirectionArrow.Instance.HideArrow();

        queueManager.PlayerHasKicked(gameObject);
    }
}