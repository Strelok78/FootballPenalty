using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    public float playerMoveSpeed = 5f;
    public float minKickForce = 5f;
    public float maxKickForce = 15f;
    public float playerZSpacing = -2f;

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
    private PlayerController activePlayer = null; // Добавлено: ссылка на активного игрока
    private float timeSinceLastReduction = 0f; // Время с последнего уменьшения интервала

    // Добавлено: отображение текущего максимального интервала в инспекторе
    [Header("Debug")]
    [SerializeField]
    private float currentMaxSpawnInterval;

    private void Start()
    {
        InitializeGame();
        CalculateNextSpawnTime();
        currentMaxSpawnInterval = maxSpawnInterval; // Инициализируем отображаемое значение
    }

    private void InitializeGame()
    {
        if (playerPrefab == null || ballPrefab == null || goal == null || playerSpawnPoint == null || ballSpawnPoint == null)
        {
            Debug.LogError("GameManager: Not all required game objects are assigned!");
            return;
        }

        ballObject = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, ballSpawnPoint);

        // Создаем только первого игрока при инициализации
        CreatePlayer();
        // Удаляем вызов SetActivePlayer
        // SetActivePlayer(players[0]); 

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

        // Уменьшаем максимальное время появления с интервалом
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
        nextSpawnTime = Random.Range(minSpawnInterval, currentMaxSpawnInterval); // Используем текущий максимальный интервал
        timeSinceLastSpawn = 0f;
    }

    private void CreatePlayer()
    {
        Vector3 spawnPosition = playerSpawnPoint.position + Vector3.forward * (players.Count * playerZSpacing);
        // Instantiate() должен принимать префаб игрока
        GameObject playerObject = Instantiate(playerPrefab, spawnPosition, playerSpawnPoint.rotation, playerSpawnPoint);
        PlayerController newPlayer = playerObject.GetComponent<PlayerController>();

        if (newPlayer == null)
        {
            Debug.LogError("GameManager: PlayerController not found on instantiated player prefab!");
            return;
        }

        Debug.Log($"GameManager: Player created at: {spawnPosition}, Total players: {players.Count}");
        players.Add(newPlayer);

        // Здесь вызываем Initialize
        newPlayer.Initialize(ballObject.transform, goal, playerMoveSpeed, minKickForce, maxKickForce);

        // Получаем PlayerClickHandler и передаем ссылку на GameManager
        PlayerClickHandler playerClickHandler = playerObject.GetComponent<PlayerClickHandler>();
        if (playerClickHandler != null)
        {
            playerClickHandler.gameManager = this;
        }
        else
        {
            Debug.LogError("GameManager: PlayerClickHandler not found on instantiated player prefab!");
        }
    }

    public void ResetGame()
    {
        // Перемещаем мяч в начальное положение
        ballObject.transform.localPosition = Vector3.zero;

        // Сбрасываем физику мяча
        Rigidbody ballRigidbody = ballObject.GetComponent<Rigidbody>();
        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;         // Обнуляем скорость
            ballRigidbody.angularVelocity = Vector3.zero;  // Обнуляем вращение
            // Если нужно - можно отключить гравитацию, а затем включить в BallMovement при ударе
            // ballRigidbody.useGravity = false;  
        }
        else
        {
            Debug.LogError("GameManager: Ball Rigidbody not found!");
        }

        // Удаляем первого игрока (который только что ударил)
        if (players.Count > 0)
        {
            Destroy(players[0].gameObject);
            players.RemoveAt(0);

            // Сдвигаем оставшихся игроков в очереди ближе к мячу
            MovePlayersInQueue();
        }
    }

    // Вариант 2: Перемещение игроков через корутину
    private void MovePlayersInQueue()
    {
        StartCoroutine(MovePlayersSmoothly());
        Debug.Log("GameManager: Starting smooth player queue movement.");
    }

    private IEnumerator MovePlayersSmoothly()
    {
        float moveDuration = 0.5f; // Время перемещения в секундах
        float elapsedTime = 0f;

        // Создаем копию списка игроков
        List<PlayerController> playersToMove = new List<PlayerController>(players);

        Dictionary<PlayerController, Vector3> targetPositions = new Dictionary<PlayerController, Vector3>();
        for (int i = 0; i < playersToMove.Count; i++)
        {
            targetPositions[playersToMove[i]] = playerSpawnPoint.position + Vector3.forward * (i * playerZSpacing);
        }

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration; // Нормализованное время от 0 до 1

            for (int i = 0; i < playersToMove.Count; i++)
            {
                playersToMove[i].transform.position = Vector3.Lerp(playersToMove[i].transform.position,
                    targetPositions[playersToMove[i]], t);
            }

            yield return null; // Ждем следующий кадр
        }

        // Убедимся, что игроки точно на своих местах после перемещения и устанавливаем isActive
        for (int i = 0; i < playersToMove.Count; i++)
        {
            playersToMove[i].transform.position = targetPositions[playersToMove[i]];
            playersToMove[i].isActive = (i == 0); // Новый активный игрок - первый в очереди
        }

        Debug.Log("GameManager: Finished smooth player queue movement.");
    }

    // Метод для установки активного игрока (будет вызываться после нажатия)
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
        Time.timeScale = 0; // Останавливаем время
        // Дополнительно: Останавливаем мяч, если он должен остановиться вместе с игрой
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