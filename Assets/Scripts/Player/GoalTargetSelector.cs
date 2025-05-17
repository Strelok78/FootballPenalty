using UnityEngine;

namespace Player
{
    public class GoalTargetSelector : MonoBehaviour
    {
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

        public (Vector3, float) SelectTargetPoint(float minForce, float maxForce)
        {
            GameObject goal = FindGoalObject();
            if (goal != null)
            {
                Collider goalCollider = goal.GetComponent<Collider>();
                if (goalCollider != null)
                {
                    Vector3 center = goalCollider.bounds.center;
                    Vector3 extents = goalCollider.bounds.extents;

                    float randomX = Random.Range(center.x - extents.x, center.x + extents.x);
                    float randomY = Random.Range(center.y - extents.y, center.y + extents.y);
                    float goalZ = center.z + extents.z;

                    Vector3 targetPoint = new Vector3(randomX, randomY, goalZ);
                    float kickForce = Random.Range(minForce, maxForce);

                    return (targetPoint, kickForce);
                }
                else
                {
                    Debug.LogError("GoalTargetSelector: Goal collider not found!");
                    return (Vector3.forward * 10f, maxForce);
                }
            }
            else
            {
                Debug.LogError("GoalTargetSelector: Goal object not assigned!");
                return (Vector3.forward * 10f, maxForce);
            }
        }
    }
}