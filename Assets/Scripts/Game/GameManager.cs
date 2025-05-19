using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject playerPrefab;
    public GameObject ballPrefab;
    public GameObject goal;

    [Header("Spawns")]
    public Transform playerSpawnPoint;
    public Transform ballSpawnPoint;

    [Header("Player Spawn Timing")]
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 10f;

    [Tooltip("Уменьшение максимального интервала появления каждые X секунд.")]
    public float intervalReductionInterval = 5f;
    [Tooltip("Значение, на которое уменьшается максимальный интервал.")]
    public float maxIntervalReductionValue = 0.5f;

    private List<PlayerController> players = new List<PlayerController>();
    private GameObject ballObject;
    private float timeSinceLastSpawn = 0f;
    private float nextSpawnTime = 0f;
    public PlayerController activePlayer = null;
    private float timeSinceLastReduction = 0f;

    [Header("Debug")]
    [SerializeField]
    private float currentMaxSpawnInterval;

    private void Start()
    {
        InitializeGame();
        CalculateNextSpawnTime();
        currentMaxSpawnInterval = maxSpawnInterval;
    }

    private void InitializeGame()
    {
        if (playerPrefab == null || ballPrefab == null || goal == null || playerSpawnPoint == null || ballSpawnPoint == null)
        {
            Debug.LogError("GameManager: Not all required game objects are assigned!");
            return;
        }

        ballObject = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, ballSpawnPoint);
        CreatePlayer();

        BallCollisionHandler ballCollisionHandler = ballObject.GetComponent<BallCollisionHandler>();
        if (ballCollisionHandler != null)
        {
            ballCollisionHandler.gameManager = this;
        }
        else
        {
            Debug.LogError("GameManager: BallCollisionHandler not found on the instantiated ball!");
        }
    }

    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= nextSpawnTime)
        {
            CreatePlayer();
            CalculateNextSpawnTime();
        }

        timeSinceLastReduction += Time.deltaTime;
        if (timeSinceLastReduction >= intervalReductionInterval)
        {
            ReduceMaxSpawnInterval();
            timeSinceLastReduction = 0f;
        }
    }

    private void ReduceMaxSpawnInterval()
    {
        currentMaxSpawnInterval = Mathf.Max(minSpawnInterval, currentMaxSpawnInterval - maxIntervalReductionValue);
        Debug.Log($"GameManager: Max spawn interval reduced to {currentMaxSpawnInterval}");
    }

    private void CalculateNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnInterval, currentMaxSpawnInterval);
        timeSinceLastSpawn = 0f;
    }

   private void CreatePlayer()
    {
        //  Перед итерацией создаём новый список, куда не попадут уничтоженные игроки.
        List<PlayerController> validPlayers = new List<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player != null) // Проверка на null (уничтоженные объекты == null)
            {
                validPlayers.Add(player);
            }
        }
        players = validPlayers;  // Обновляем список игроков

        int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Изменено:  Используем остаток от деления, чтобы организовать игроков в несколько рядов (пример).
            //  Можно изменить формулу для получения другой схемы расположения.
            int row = players.Count / 3; // Например, 3 игрока в ряд
            int col = players.Count % 3;
            Vector3 spawnPosition = playerSpawnPoint.position + Vector3.forward * (row * -2f) + Vector3.right * (col * 1.5f);
            
            //  Вместо:
            //  Vector3 spawnPosition = playerSpawnPoint.position + Vector3.forward * (players.Count * playerZSpacing);
            
            bool positionTaken = false;
            foreach (PlayerController existingPlayer in players)
            {
                if (Vector3.Distance(spawnPosition, existingPlayer.transform.position) < 0.5f)
                {
                    positionTaken = true;
                    break;
                }
            }

            if (!positionTaken)
            {
                GameObject playerObject = Instantiate(playerPrefab, spawnPosition, playerSpawnPoint.rotation, playerSpawnPoint);
                PlayerController newPlayer = playerObject.GetComponent<PlayerController>();

                if (newPlayer == null)
                {
                    Debug.LogError("GameManager: PlayerController not found on instantiated player prefab!");
                    return;
                }

                Debug.Log($"GameManager: Player created at: {spawnPosition}, Total players: {players.Count}");
                players.Add(newPlayer);
                newPlayer.Initialize(ballObject.transform, goal);

                PlayerClickHandler playerClickHandler = playerObject.GetComponent<PlayerClickHandler>();
                if (playerClickHandler != null)
                {
                    playerClickHandler.gameManager = this;
                }
                else
                {
                    Debug.LogError("GameManager: PlayerClickHandler not found on instantiated player prefab!");
                }
                return;
            }
            else
            {
                Debug.LogWarning($"GameManager: Spawn position taken, trying to adjust.");
            }
        }
        Debug.LogError("GameManager: Could not find a free spawn position after " + maxAttempts + " attempts!");
    }
    public void ResetGame()
    {
        ballObject.transform.localPosition = Vector3.zero;
        ballObject.SetActive(true);

        Rigidbody ballRigidbody = ballObject.GetComponent<Rigidbody>();
        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.WakeUp();
        }
        else
        {
            Debug.LogError("GameManager: Ball Rigidbody not found!");
        }
    }

    public void SetActivePlayer(PlayerController player)
    {
        // if (activePlayer != null)
        // {
        //     activePlayer.isActive = false;
        // }
        activePlayer = player;
        if (activePlayer != null)
        {
            activePlayer.isActive = true;
            activePlayer.OnPlayerClicked();
        }
        Debug.Log($"GameManager: Set active player to: " + (player != null ? player.gameObject.name : "none"));
    }
    
    public void HandlePlayerCollision()
    {
        Debug.Log("GameManager: Player collision detected. Stopping the game.");
        Time.timeScale = 0;
        if (ballObject != null)
        {
            Rigidbody ballRigidbody = ballObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
    // Дополнительно: Метод для возобновления игры
    public void ResumeGame()
    {
        Debug.Log("GameManager: Resuming the game.");
        Time.timeScale = 1;
    }
}