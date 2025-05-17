using System;
using UnityEngine;

public class PlayerClickHandler : MonoBehaviour
{
    private PlayerController playerController;
    [NonSerialized] public GameManager gameManager;
    private Camera mainCamera;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerClickHandler: PlayerController component not found on the player prefab!");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerClickHandler: Main camera not found!");
            enabled = false;
            return;
        }

        // Исправлено: Используем FindAnyObjectByType
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("PlayerClickHandler: GameManager not found on the scene!");
            enabled = false;
            return;
        }
    }

    // Заменяем OnClickPerformed на Update
    private void Update()
    {
        // Обработка нажатия левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick(Vector2? touchPos = null)
    {
        Vector2 clickPosition = touchPos ?? Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(clickPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Player")))  // Убедитесь в правильном слое
        {
            if (hit.collider.gameObject == gameObject)
            {
                gameManager.SetActivePlayer(playerController);
            }
            else
            {
                Debug.Log("PlayerClickHandler: Clicked on something else: " + hit.collider.gameObject.name); // Лог попадания по другому объекту
            }
        }
        else
        {
            Debug.Log("PlayerClickHandler: Raycast hit nothing."); // Лог отсутствия попаданий
        }
    }
}