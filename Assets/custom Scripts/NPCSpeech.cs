using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCSpeech : MonoBehaviour
{
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

    [Tooltip("Historical location image shown in the speech bubble.")]
    public Sprite locationImage;

    [Tooltip("Caption for the location image, e.g. 'Old Dharahara, 1934'")]
    public string locationCaption = "";

    [Header("Interaction")]
    public float interactRange = 4f;
    public LayerMask npcLayer;

    [Header("Speech Bubble UI (shared across all NPCs)")]
    public GameObject speechBubbleRoot;
    public TextMeshProUGUI bubbleSpeakerName;
    public TextMeshProUGUI bubbleSpeakerRole;
    public TextMeshProUGUI bubbleQuoteText;
    public Image bubblePortrait;
    public GameObject portraitObj;
    public Button bubbleCloseButton;

    [Header("Location Image UI")]
    public Image bubbleLocationImage;
    public TextMeshProUGUI bubbleLocationCaption;
    public GameObject locationImageSection;

    [Header("Interaction Prompt")]
    public GameObject promptRoot;
    public TextMeshProUGUI promptText;

    private Camera _cam;
    private bool _open;
    private static NPCSpeech _currentOpen;

    void Start()
    {
        _cam = Camera.main;
        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
        if (promptRoot != null) promptRoot.SetActive(false);
        if (locationImageSection != null) locationImageSection.SetActive(false);
        if (bubbleCloseButton != null) bubbleCloseButton.onClick.AddListener(CloseBubble);
    }

    void Update()
    {
        HandlePrompt();

        // E key — open if not open, close if already open
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_open)
                CloseBubble();
            else
                TrySpeak();
        }

        // Escape key — always closes if open
        if (Input.GetKeyDown(KeyCode.Escape) && _open)
            CloseBubble();
    }

    void HandlePrompt()
    {
        if (promptRoot == null || _cam == null || _open) return;
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool aimed = Physics.Raycast(ray, out RaycastHit hit, interactRange)
                     && hit.collider != null
                     && hit.collider.gameObject == gameObject;
        promptRoot.SetActive(aimed);
        if (aimed && promptText != null)
            promptText.text = $"[E]  Talk to {speakerName}";
    }

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
        if (bubbleQuoteText != null) bubbleQuoteText.text = $"\"{quote}\"";

        bool hasPortrait = portrait != null;
        if (portraitObj != null) portraitObj.SetActive(hasPortrait);
        if (hasPortrait && bubblePortrait != null) bubblePortrait.sprite = portrait;

        bool hasLocationImage = locationImage != null;
        if (locationImageSection != null) locationImageSection.SetActive(hasLocationImage);
        if (hasLocationImage)
        {
            if (bubbleLocationImage != null) bubbleLocationImage.sprite = locationImage;
            if (bubbleLocationCaption != null) bubbleLocationCaption.text = locationCaption;
        }

        speechBubbleRoot.SetActive(true);
        if (promptRoot != null) promptRoot.SetActive(false);

        _open = true;
        _currentOpen = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseBubble()
    {
        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
        _open = false;
        if (_currentOpen == this) _currentOpen = null;
        StartCoroutine(LockCursorNextFrame());
    }

    IEnumerator LockCursorNextFrame()
    {
        yield return null;
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
