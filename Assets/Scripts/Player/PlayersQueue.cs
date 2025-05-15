using System.Collections.Generic;
using UnityEngine;

public class PlayersQueue : MonoBehaviour
{
    public GameObject playerPrefab;
    public int initialCount = 5;
    public float spacing = 1.5f;

    private Queue<GameObject> playerQueue = new Queue<GameObject>();
    public Transform spawnPoint; // Где будет первый игрок (например, позади мяча по Z)

    private void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            SpawnPlayer(i);
        }

        ActivateNextPlayer();
    }

    void SpawnPlayer(int index)
    {
        Vector3 position = spawnPoint.position - new Vector3(0, 0, index * spacing);
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<PlayerController>().SetQueueManager(this);
        playerQueue.Enqueue(player);
    }

    public void ActivateNextPlayer()
    {
        if (playerQueue.Count > 0)
        {
            GameObject next = playerQueue.Peek();
            next.GetComponent<PlayerController>().EnableInput();
        }
    }

    public void PlayerHasKicked(GameObject player)
    {
        if (playerQueue.Count > 0 && playerQueue.Peek() == player)
        {
            playerQueue.Dequeue();
            Destroy(player);
            ActivateNextPlayer();
        }
    }
}