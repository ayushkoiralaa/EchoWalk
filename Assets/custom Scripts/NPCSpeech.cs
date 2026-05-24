using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCSpeech : MonoBehaviour
{
    // ── NPC content ───────────────────────────────────────────────────────────

    [Header("NPC Info")]
    public string speakerName = "Ramesh Shrestha";
    public string speakerRole = "Local Resident";

    [TextArea(3, 6)]
    public string quote =
        "I was born within sight of the old tower. Every morning I would " +
        "look up and see it. When it fell, it felt like losing a member of " +
        "the family. The new one is beautiful — but it carries our grief too.";

    [Tooltip("Optional portrait image of the NPC.")]
    public Sprite portrait;

    // ── Interaction ───────────────────────────────────────────────────────────

    [Header("Interaction")]
    public float interactRange = 4f;
    public LayerMask npcLayer;

    // ── Shared Speech Bubble UI ───────────────────────────────────────────────

    [Header("Speech Bubble UI (shared across all NPCs)")]
    public GameObject      speechBubbleRoot;
    public TextMeshProUGUI bubbleSpeakerName;
    public TextMeshProUGUI bubbleSpeakerRole;
    public TextMeshProUGUI bubbleQuoteText;
    public Image           bubblePortrait;
    public GameObject      portraitObj;       // Parent holding portrait image
    public Button          bubbleCloseButton;

    [Header("Interaction Prompt")]
    public GameObject      promptRoot;
    public TextMeshProUGUI promptText;

    // ── Private ───────────────────────────────────────────────────────────────

    private Camera   _cam;
    private bool     _open;
    private static NPCSpeech _currentOpen;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _cam = Camera.main;

        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
        if (promptRoot       != null) promptRoot.SetActive(false);
        if (bubbleCloseButton!= null) bubbleCloseButton.onClick.AddListener(CloseBubble);
    }

    void Update()
    {
        HandlePrompt();

        if (Input.GetKeyDown(KeyCode.E))
            TrySpeak();
    }

    // ── Crosshair prompt ──────────────────────────────────────────────────────

    void HandlePrompt()
    {
        if (promptRoot == null || _cam == null || _open) return;

        Ray  ray  = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool aimed = Physics.Raycast(ray, out RaycastHit hit, interactRange)
                     && hit.collider != null
                     && hit.collider.gameObject == gameObject;

        promptRoot.SetActive(aimed);

        if (aimed && promptText != null)
            promptText.text = $"[E]  Talk to {speakerName}";
    }

    // ── Speech bubble ─────────────────────────────────────────────────────────

    void TrySpeak()
    {
        if (_open || _cam == null) return;

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;
        if (hit.collider.gameObject != gameObject) return;

        OpenBubble();
    }

    void OpenBubble()
    {
        if (speechBubbleRoot == null) return;

        if (_currentOpen != null && _currentOpen != this)
            _currentOpen.CloseBubble();

        if (bubbleSpeakerName != null) bubbleSpeakerName.text = speakerName;
        if (bubbleSpeakerRole != null) bubbleSpeakerRole.text = speakerRole;
        if (bubbleQuoteText   != null) bubbleQuoteText.text   = $"\"{quote}\"";

        bool hasPortrait = portrait != null;
        if (portraitObj    != null) portraitObj.SetActive(hasPortrait);
        if (hasPortrait && bubblePortrait != null) bubblePortrait.sprite = portrait;

        speechBubbleRoot.SetActive(true);
        if (promptRoot != null) promptRoot.SetActive(false);

        _open        = true;
        _currentOpen = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void CloseBubble()
    {
        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
        _open = false;
        if (_currentOpen == this) _currentOpen = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}
