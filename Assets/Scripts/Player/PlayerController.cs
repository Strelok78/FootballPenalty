using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[System.Serializable]
public class KickEvent : UnityEvent<InputAction.CallbackContext> {}

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float kickForce = 10f;
    public Transform ball; // Ссылка на объект мяча
    public Transform initialBallPosition;
    public Transform initialPlayerPosition;
    public bool canKick = true; // Флаг, разрешающий удар (public)
    private AimArrowController aimArrowController; // Ссылка на контроллер стрелки
    private bool isMovingToBall = false; // Флаг, показывающий, что игрок бежит к мячу
    private Vector3 targetPosition; // Объявляем targetPosition на уровне класса

    public KickEvent onKick;

    void Start()
    {
        if (ball != null)
        {
            aimArrowController = ball.GetComponent<AimArrowController>();
            if (aimArrowController == null)
            {
                Debug.LogError("AimArrowController не найден на объекте Ball!");
            }
            else
            {
                aimArrowController.ShowArrow();
                aimArrowController.SetIsAiming(true); // Стрелка двигается при старте
            }
        }
        else
        {
            Debug.LogError("Не назначена ссылка на объект Ball!");
        }
    }

    void Update()
    {
        if (isMovingToBall)
        {
            Vector3 directionToBall = (targetPosition - transform.position).normalized;
            transform.position += directionToBall * moveSpeed * Time.deltaTime;

            float distanceToBall = Vector3.Distance(transform.position, targetPosition);
            if (distanceToBall < 0.6f)
            {
                isMovingToBall = false;
                KickBall(aimArrowController.GetKickDirection());
                canKick = false;
                aimArrowController.HideArrow();
            }
            else
            {
                if (aimArrowController != null)
                {
                    aimArrowController.SetIsAiming(false); // Стрелка фиксируется во время бега
                }
            }
        }
        else
        {
            if (aimArrowController != null)
            {
                aimArrowController.SetIsAiming(true); // Стрелка двигается, когда игрок стоит
            }
        }
    }

    public void OnKickInput(InputAction.CallbackContext context)
    {
        if (context.performed && canKick)
        {
            if (!isMovingToBall)
            {
                // Начинаем движение к мячу
                isMovingToBall = true;
                targetPosition = ball.position;
                if (aimArrowController != null)
                {
                    aimArrowController.SetIsAiming(false); // Фиксируем стрелку, когда игрок начинает движение
                }
            }
            else
            {
                // Останавливаем движение к мячу
                isMovingToBall = false;
                if (aimArrowController != null)
                {
                    aimArrowController.SetIsAiming(true); // Стрелка двигается, когда игрок стоит
                }
            }
        }
    }

    void KickBall(Vector3 kickDirection)
    {
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("У объекта Ball отсутствует компонент Rigidbody!");
        }
    }

    public void ResetPlayerPosition()
    {
        if (initialPlayerPosition != null)
        {
            transform.position = initialPlayerPosition.position;
            canKick = true;
            isMovingToBall = false;
            if (aimArrowController != null)
            {
                aimArrowController.ShowArrow();
                aimArrowController.SetIsAiming(true);
            }
            else if (ball != null && ball.transform.Find("ArrowPivot") != null)
            {
                ball.transform.Find("ArrowPivot").gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Не установлена initialPlayerPosition для Player.");
        }
    }
}