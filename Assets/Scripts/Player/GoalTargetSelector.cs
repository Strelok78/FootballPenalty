using UnityEngine;

namespace Player
{
    public class GoalTargetSelector : MonoBehaviour
    {
        [Header("Goal Sections")]
        [SerializeField] private Material[] sectionMaterials = new Material[3];

        // Метод для поиска цели по тегу
        private GameObject FindGoalObject()
        {
            GameObject goalObject = GameObject.FindGameObjectWithTag("Goal");
            if (goalObject == null)
            {
                Debug.LogError("GoalTargetSelector: No object with tag 'Goal' found!");
            }
            return goalObject;
        }

        public (Vector3, float, int) SelectTargetPoint(float minForce, float maxForce)
        {
            GameObject goal = FindGoalObject();
            if (goal != null)
            {
                Collider goalCollider = goal.GetComponent<Collider>();
                if (goalCollider != null)
                {
                    Bounds goalBounds = goalCollider.bounds;
                    // Разделяем ворота на 3 зоны (вертикальные)
                    float sectionWidth = goalBounds.size.x / 3f;

                    // Выбираем случайную зону
                    int sectionIndex = Random.Range(0, 3);

                    // Вычисляем центр выбранной зоны
                    Vector3 sectionCenter = goalBounds.center;
                    sectionCenter.x += sectionWidth * (sectionIndex - 1); // Смещаем по X относительно центра

                    // Случайная точка внутри выбранной зоны
                    Vector3 targetPoint = sectionCenter + new Vector3(Random.Range(-sectionWidth / 2f, sectionWidth / 2f),
                                                                   Random.Range(-goalBounds.size.y / 2f, goalBounds.size.y / 2f), // Используем полную высоту ворот
                                                                   goalBounds.extents.z);

                    float kickForce = Random.Range(minForce, maxForce);

                    // Возвращаем ещё и номер секции
                    return (targetPoint, kickForce, sectionIndex);
                }
                else
                {
                    Debug.LogError("GoalTargetSelector: Goal collider not found!");
                    return (Vector3.forward * 10f, maxForce, -1); // Возвращаем -1 как индикатор ошибки
                }
            }
            else
            {
                Debug.LogError("GoalTargetSelector: Goal object not assigned!");
                return (Vector3.forward * 10f, maxForce, -1); // Возвращаем -1 как индикатор ошибки
            }
        }

        // Метод для получения материала секции по индексу
        public Material GetSectionMaterial(int index)
        {
            if (index >= 0 && index < sectionMaterials.Length)
            {
                return sectionMaterials[index];
            }
            else
            {
                Debug.LogError($"GoalTargetSelector: Invalid section index {index}.");
                return null; // или возвращайте материал по умолчанию
            }
        }
    }
}