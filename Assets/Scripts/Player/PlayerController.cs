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
    private float moveSpeed;
    private float minKickForce;
    private float maxKickForce;
    private Transform ball;
    private GameObject goal;
    public PlayerState currentState = PlayerState.Idle;
    private Player.GoalTargetSelector goalTargetSelector;
    private float variableKickForce;
    private Vector3 kickTargetPoint;
    private Vector3 moveDirection = Vector3.zero;
    private bool isMoving = false;

    public bool IsMoving => isMoving;
    public bool isActive = false;

    public void Initialize(Transform ballTransform, GameObject goalObject, float moveSpeedValue, float minForce, float maxForce)
    {
        ball = ballTransform;
        goal = goalObject;
        moveSpeed = moveSpeedValue;
        minKickForce = minForce;
        maxKickForce = maxForce;

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
                isMoving = false;  //  <--  ВАЖНО:  Останавливаем движение!
                Debug.Log($"Player {gameObject.name}: Potential collision detected. Temporarily stopping.");
            }
            else
            {
                MoveToBall();
            }
        }
        Debug.Log($"Player: {gameObject.name}, isActive: {isActive}, currentState: {currentState}, isMoving: {isMoving}");
    }

    private void MoveToBall()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float distanceToBallXZ = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(ball.position.x, ball.position.z));
        if (distanceToBallXZ < 0.6f)
        {
            currentState = PlayerState.Kicked;
            KickBall();
            isMoving = false;
        }
    }

    private Vector3 CalculateDirectionToTarget(Vector3 start, Vector3 target)
    {
        return new Vector3(target.x - start.x, 0f, target.z - start.z).normalized;
    }

    // Изменено: Убрана логика переключения isActive.
    public void OnPlayerClicked()
    {
        Debug.Log($"Player {gameObject.name} clicked. isActive: {isActive}");

        if (currentState == PlayerState.CanKick)
        {
            if (!isMoving)
            {
                currentState = PlayerState.MovingToBall;
                moveDirection = CalculateDirectionToTarget(transform.position, ball.position);
                isMoving = true;
            }
            else
            {
                StopMoving();
            }
        }
        else if (currentState == PlayerState.MovingToBall && isMoving)
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        currentState = PlayerState.CanKick;
        // Сообщаем GameManager, что игрок больше не активен:
        FindAnyObjectByType<GameManager>().SetActivePlayer(null); // передаем null для сброса активного игрока
    }

    public void PrepareForNextKick()
    {
        if (goalTargetSelector != null)
        {
            (Vector3 targetPoint, float kickForce, int materialIndex) = goalTargetSelector.SelectTargetPoint(minKickForce, maxKickForce);
            kickTargetPoint = targetPoint;
            variableKickForce = kickForce;
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Material[] materials = meshRenderer.materials;
            materials[0] = Instantiate(goalTargetSelector.GetSectionMaterial(materialIndex));
            meshRenderer.materials = materials;
            Debug.Log("SSSS - " + materials.Length);
        }
    }

    void KickBall()
    {
        BallMovement ballMovement = ball.GetComponent<BallMovement>();
        if (ballMovement != null)
        {
            ballMovement.SetTarget(kickTargetPoint, variableKickForce);
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