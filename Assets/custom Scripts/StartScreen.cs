using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root panel of the entire start screen.")]
    public GameObject startPanel;

    [Tooltip("The 'Begin Exploration' button.")]
    public Button startButton;

    void Start()
    {
        if (startPanel != null) startPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisableMovement(true);
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    void OnStartClicked()
    {
        if (startPanel != null) startPanel.SetActive(false);
        DisableMovement(false);
        StartCoroutine(LockCursorNextFrame()); // FIX: wait 2 frames before locking
        if (MusicManager.Instance != null)
            MusicManager.Instance.OnGameStart();
    }

    IEnumerator LockCursorNextFrame()
    {
        yield return null; // wait frame 1
        yield return null; // wait frame 2
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableMovement(bool disable)
    {
        // Finds every MonoBehaviour on any object with "Camera" or "Player"
        // in its name and enables/disables it — skips this script itself
        MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour mb in all)
        {
            if (mb == this) continue;
            string n = mb.gameObject.name;
            if (n.Contains("Camera") || n.Contains("Player") || n.Contains("Follow"))
            {
                mb.enabled = !disable;
            }
        }
    }
}
