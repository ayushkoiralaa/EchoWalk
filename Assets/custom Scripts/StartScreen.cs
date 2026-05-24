using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class StartScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root panel of the entire start screen.")]
    public GameObject startPanel;

    [Tooltip("The 'Begin Exploration' button.")]
    public Button startButton;

    private bool _gameStarted = false;

    void Start()
    {
        if (startPanel != null) startPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableMovement(true);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    void Update()
    {
        // If game hasn't started yet, keep cursor unlocked always
        // This handles screen resize resetting cursor state
        if (!_gameStarted)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnStartClicked()
    {
        if (_gameStarted) return; // prevent double firing
        _gameStarted = true;

        if (startPanel != null) startPanel.SetActive(false);

        DisableMovement(false);

        StartCoroutine(LockCursorNextFrame());

        if (MusicManager.Instance != null)
            MusicManager.Instance.OnGameStart();
    }

    IEnumerator LockCursorNextFrame()
    {
        // Wait extra frames to let button click and any
        // screen resize events fully finish before locking
        yield return null;
        yield return null;
        yield return null;

        // Force EventSystem to clear any lingering selection
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableMovement(bool disable)
    {
        MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour mb in all)
        {
            if (mb == this) continue;
            string n = mb.gameObject.name;
            if (n.Contains("Camera") || n.Contains("Player") || n.Contains("Follow"))
                mb.enabled = !disable;
        }
    }
}
