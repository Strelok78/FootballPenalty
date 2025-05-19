using UnityEngine;

public enum PlayerState
{
    Idle,
    MovingToBall,
    CanKick,
    Kicked
}

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float minKickForce = 5f;
    [SerializeField] private float maxKickForce = 15f;

    private Transform ball;
    private GameObject goal;
    public PlayerState currentState = PlayerState.Idle;
    private Player.GoalTargetSelector goalTargetSelector;
    private float variableKickForce;
    private Vector3 kickTargetPoint;
    private Vector3 moveDirection = Vector3.zero;
    private bool isMoving = false;
    private Vector3 targetPosition;

    public bool IsMoving => isMoving;
    public bool isActive = false;

    public void Initialize(Transform ballTransform, GameObject goalObject)
    {
        ball = ballTransform;
        goal = goalObject;

        goalTargetSelector = GetComponent<Player.GoalTargetSelector>();
        if (goalTargetSelector == null)
        {
            Debug.LogError("PlayerController: GoalTargetSelector not found on the player prefab!");
        }

        PrepareForNextKick();
        currentState = PlayerState.CanKick;
    }

    private void Update()
    {
        // Добавлено: Проверка на столкновения с другими игроками
        if (isActive && currentState == PlayerState.MovingToBall && isMoving)
        {
            if (CheckForPlayerCollision())
            {
                // Временно останавливаем текущего игрока, если есть столкновение
                moveDirection = Vector3.zero;
                isMoving = false;
                Debug.Log($"Player {gameObject.name}: Potential collision detected. Temporarily stopping.");
            }
            else
            {
                MoveToTarget();
            }
        }
        Debug.Log($"Player: {gameObject.name}, isActive: {isActive}, currentState: {currentState}, isMoving: {isMoving}");
    }
    
    private void MoveToTarget()
    {
        // Запоминаем текущую позицию игрока по Y
        float currentY = transform.position.y;
        
        // Перемещаем игрока только по X и Z
        Vector3 nextPosition = Vector3.MoveTowards(new Vector3(transform.position.x, 0, transform.position.z), 
                                                 new Vector3(targetPosition.x, 0, targetPosition.z), 
                                                 moveSpeed * Time.deltaTime);

        transform.position = new Vector3(nextPosition.x, currentY, nextPosition.z);

        //  Проверяем расстояние по XZ, чтобы не учитывать Y.
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPosition.x, targetPosition.z)) < 0.1f)
        {
            if (currentState == PlayerState.MovingToBall)
            {
                currentState = PlayerState.Kicked;
                KickBall();
                isMoving = false;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    // Метод для первого игрока (движение прямо к мячу)
    private void SetTargetForMoveToBall()
    {
        targetPosition = ball.position;
    }

    // Метод для остальных игроков (обход очереди и движение к мячу)
    public void MoveAroundQueueAndToBall()
    {
        if (currentState != PlayerState.CanKick) return; // Двигаемся только если можно бить

        currentState = PlayerState.MovingToBall;
        isMoving = true;

        // Определяем позицию в очереди (предполагаем, что игроки располагаются вдоль Z)
        float queuePositionZ = transform.position.z;
        Vector3 spawnPointPosition = FindAnyObjectByType<GameManager>().playerSpawnPoint.position;

        // Точки обхода
        Vector3 rightOfQueue = spawnPointPosition + Vector3.right * 3f + Vector3.forward * queuePositionZ;  // Справа от очереди
        Vector3 behindBall = ball.position + Vector3.back * 2f;  // Позади мяча
        
        StartCoroutine(MoveToPoints(new Vector3[] { rightOfQueue, behindBall, ball.position }));
    }

    private System.Collections.IEnumerator MoveToPoints(Vector3[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            targetPosition = points[i];
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                yield return null;
            }
        }
        KickBall();
        isMoving = false;
    }


    private Vector3 CalculateDirectionToTarget(Vector3 start, Vector3 target)
    {
        return new Vector3(target.x - start.x, 0f, target.z - start.z).normalized;
    }
    
    public void OnPlayerClicked()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager.activePlayer == null || gameManager.activePlayer == this) // Если нет активного или клик по активному
        {
            if (currentState == PlayerState.CanKick && !isMoving)
            {
                currentState = PlayerState.MovingToBall;
                moveDirection = CalculateDirectionToTarget(transform.position, ball.position);
                isMoving = true;
                SetTargetForMoveToBall();
            }
        }
        else
        {
            MoveAroundQueueAndToBall(); // Движение для игроков не из очереди
        }
    }
    
    private void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        currentState = PlayerState.CanKick;
    }

    public void PrepareForNextKick()
    {
        if (goalTargetSelector != null)
        {
            (Vector3 targetPoint, float kickForce) = goalTargetSelector.SelectTargetPoint(minKickForce, maxKickForce);
            kickTargetPoint = targetPoint;
            variableKickForce = kickForce;
        }
    }

    void KickBall()
    {
        BallMovement ballMovement = ball.GetComponent<BallMovement>();
        if (ballMovement != null)
        {
            ballMovement.SetTarget(kickTargetPoint, variableKickForce);
            // Добавлено: Удаление игрока
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PlayerController: BallMovement component not found on the ball!");
        }
    }

    // Новый метод проверки столкновений
    private bool CheckForPlayerCollision()
    {
        float minDistance = 0.4f;  // Уменьшено минимальное расстояние между игроками
        // Исправлено: Используем FindObjectsByType
        foreach (PlayerController otherPlayer in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (otherPlayer != this && (otherPlayer.isMoving || isMoving))  // Проверяем всех двигающихся игроков, и тех, кто уже в движении.
            {
                float distance = Vector3.Distance(transform.position, otherPlayer.transform.position);
                if (distance < minDistance)
                {
                    // Сообщаем GameManager о столкновении
                    FindAnyObjectByType<GameManager>().HandlePlayerCollision();
                    return true;  // Обнаружено столкновение
                }
            }
        }
        return false; // Столкновений нет
    }
}