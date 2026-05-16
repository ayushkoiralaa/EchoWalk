// ============================================================
//  StartScreen.cs  —  EchoWalk  —  Unity 6000.0.58f2
// ============================================================
//  Attach to an empty GameObject called "StartScreenManager".
//  Shows an intro screen when the game launches.
//  Clicking "Begin Exploration" hides it and starts the game.
//
//  SETUP:
//   1. Create a Canvas (Screen Space - Overlay), name it "Canvas_StartScreen"
//   2. Add a full-screen Panel child, name it "StartPanel"
//   3. Inside StartPanel add:
//        - Image (background, optional dark color or photo)
//        - TextMeshPro text for the Title
//        - TextMeshPro text for the Description
//        - Button (TextMeshPro) for "Begin Exploration"
//   4. Attach this script to an empty GameObject
//   5. Assign all references in the Inspector
//   6. Assign your Player GameObject so movement is blocked until Start is clicked
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root panel of the entire start screen.")]
    public GameObject startPanel;

    [Tooltip("The 'Begin Exploration' button.")]
    public Button startButton;

    [Header("Player")]
    [Tooltip("Drag your Player GameObject here. Movement is disabled until Start is clicked.")]
    public GameObject player;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Show the start screen
        if (startPanel != null) startPanel.SetActive(true);

        // Unlock cursor so player can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Disable player movement until game starts
        if (player != null)
        {
            var controller = player.GetComponent<FirstPersonController>();
            if (controller != null) controller.enabled = false;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // Wire button
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    // ── Button callback ───────────────────────────────────────────────────────

    void OnStartClicked()
    {
        // Hide the start screen
        if (startPanel != null) startPanel.SetActive(false);

        // Lock cursor for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Re-enable player
        if (player != null)
        {
            var controller = player.GetComponent<FirstPersonController>();
            if (controller != null) controller.enabled = true;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
        }
    }
}
