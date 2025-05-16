using UnityEngine;

public class AimArrowController : MonoBehaviour
{
    public float swingSpeed = 30f; // Скорость качания стрелки
    public float swingAngle = 45f; // Угол качания стрелки в каждую сторону
    public Transform arrowPivot; // Ссылка на Transform объекта-основания стрелки (Pivot)
    private bool isAiming = true; // Стрелка активна сразу
    private float currentAngle = 0f;
    private int swingDirection = 1; // 1 для вправо, -1 для влево
    private Quaternion initialPivotLocalRotation; // Исходное локальное вращение Pivot

    void Start()
    {
        // Проверяем, назначена ли основа стрелки
        if (arrowPivot == null)
        {
            Debug.LogError("AimArrowController: Не назначена ссылка на Transform основы стрелки (Pivot) в AimArrowController!");
            enabled = false; // Отключаем скрипт, чтобы избежать ошибок
        }
        else
        {
            arrowPivot.gameObject.SetActive(true); // Показываем Pivot при старте
            initialPivotLocalRotation = arrowPivot.localRotation; // Запоминаем исходное локальное вращение
        }
    }

    void Update()
    {
        if (isAiming && arrowPivot != null && arrowPivot.gameObject.activeSelf)
        {
            // Вычисляем целевой угол
            float targetAngle = swingDirection * swingAngle;

            // Плавно поворачиваем Pivot
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, swingSpeed * Time.deltaTime);
            arrowPivot.localEulerAngles = new Vector3(0f, currentAngle, 0f);

            // Меняем направление, когда достигаем крайних точек
            if (Mathf.Abs(currentAngle) >= swingAngle)
            {
                swingDirection *= -1;
            }
        }
    }

    // Метод для фиксации направления стрелки при нажатии пробела (вызывается из PlayerController)
    public Vector3 GetKickDirection()
    {
        isAiming = false; // Фиксируем стрелку
        if (arrowPivot != null && arrowPivot.gameObject.activeSelf)
        {
            return arrowPivot.forward;
        }
        return transform.forward; // Возвращаем направление вперед по умолчанию, если Pivot не найден или не активен
    }

    public void HideArrow()
    {
        if (arrowPivot != null)
        {
            arrowPivot.gameObject.SetActive(false);
            isAiming = false;
        }
    }

    public void ShowArrow()
    {
        if (arrowPivot != null)
        {
            arrowPivot.gameObject.SetActive(true);
            arrowPivot.localRotation = initialPivotLocalRotation; // Восстанавливаем исходное локальное вращение
            currentAngle = 0f;
            swingDirection = 1;
        }
    }

    public void SetIsAiming(bool aiming)
    {
        isAiming = aiming;
    }
}