// ============================================================
//  AreaDiscovery.cs  —  EchoWalk  —  Unity 6000.0.58f2
// ============================================================
//  Two scripts in one file:
//    1. AreaDiscoveryManager  — tracks total progress, updates HUD, shows popup
//    2. DiscoverableArea      — attach to each trigger zone in the scene
//
//  SETUP:
//   1. Create an empty GameObject "DiscoveryManager", attach AreaDiscoveryManager
//   2. Assign the HUD label (e.g. "Places Discovered: 0/7") and the completion popup
//   3. For each area in your scene:
//        - Create an empty GameObject, position it at that area
//        - Add a Sphere Collider, check "Is Trigger", set Radius to taste
//        - Attach DiscoverableArea
//        - Give it a name (e.g. "Main Entrance", "Top Balcony")
//        - Drag DiscoveryManager into the Manager field
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// ─────────────────────────────────────────────────────────────────────────────
//  MANAGER  —  attach to one empty GameObject in the scene
// ─────────────────────────────────────────────────────────────────────────────

public class AreaDiscoveryManager : MonoBehaviour
{
    public static AreaDiscoveryManager Instance { get; private set; }

    [Header("HUD")]
    [Tooltip("TMP label that shows 'Places Discovered: 0/7'. " +
             "Put this somewhere visible on screen, e.g. top-left.")]
    public TextMeshProUGUI scoreLabel;

    [Header("Area Indicator")]
    [Tooltip("Small TMP label that briefly pops up with the area name " +
             "when the player enters a new area, e.g. '📍 Main Entrance'.")]
    public TextMeshProUGUI areaIndicatorText;

    [Tooltip("How long the area name indicator stays on screen.")]
    public float indicatorDuration = 3f;

    [Header("Completion Popup")]
    [Tooltip("Root GameObject of the 'You explored all areas!' popup panel.")]
    public GameObject completionPopup;

    [Tooltip("TMP label inside the completion popup (optional — for custom message).")]
    public TextMeshProUGUI completionText;

    [Tooltip("Close/dismiss button on the completion popup.")]
    public Button completionCloseButton;

    [Header("Total Areas")]
    [Tooltip("Must match the exact number of DiscoverableArea zones in the scene.")]
    public int totalAreas = 7;

    // ── State ─────────────────────────────────────────────────────────────────

    private int _discovered = 0;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Simple singleton so DiscoverableArea can find this easily
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (areaIndicatorText != null) areaIndicatorText.gameObject.SetActive(false);
        if (completionPopup   != null) completionPopup.SetActive(false);
        if (completionCloseButton != null)
            completionCloseButton.onClick.AddListener(CloseCompletion);

        UpdateScoreLabel();
    }

    // ── Called by DiscoverableArea ────────────────────────────────────────────

    /// <summary>Called by a DiscoverableArea when the player enters it for the first time.</summary>
    public void RegisterDiscovery(string areaName)
    {
        _discovered++;
        UpdateScoreLabel();
        ShowAreaIndicator(areaName);

        if (_discovered >= totalAreas)
            StartCoroutine(ShowCompletion());
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    void UpdateScoreLabel()
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Places Discovered: {_discovered}/{totalAreas}";
    }

    void ShowAreaIndicator(string areaName)
    {
        if (areaIndicatorText == null) return;
        StopCoroutine(nameof(HideIndicatorAfterDelay));
        StartCoroutine(nameof(HideIndicatorAfterDelay), areaName);
    }

    IEnumerator HideIndicatorAfterDelay(string areaName)
    {
        areaIndicatorText.text = $"📍  {areaName}";
        areaIndicatorText.gameObject.SetActive(true);

        yield return new WaitForSeconds(indicatorDuration);

        areaIndicatorText.gameObject.SetActive(false);
    }

    IEnumerator ShowCompletion()
    {
        // Small delay so the last area indicator is visible first
        yield return new WaitForSeconds(1.5f);

        if (completionPopup != null) completionPopup.SetActive(true);

        if (completionText != null)
            completionText.text =
                "🏆  You've explored all areas!\n\n" +
                "Thank you for walking through the story of Dharahara.\n" +
                "Its legacy lives on.";

        // Unlock cursor so player can click the close button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void CloseCompletion()
    {
        if (completionPopup != null) completionPopup.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}


// ─────────────────────────────────────────────────────────────────────────────
//  DISCOVERABLE AREA  —  attach to each trigger zone in the scene
// ─────────────────────────────────────────────────────────────────────────────

public class DiscoverableArea : MonoBehaviour
{
    [Header("Area Info")]
    [Tooltip("Name shown on screen when this area is discovered, e.g. 'Main Entrance'.")]
    public string areaName = "Main Entrance";

    [Header("Manager")]
    [Tooltip("Drag the DiscoveryManager GameObject here.")]
    public AreaDiscoveryManager manager;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _discovered = false;

    // ── Trigger ───────────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (_discovered || !other.CompareTag("Player")) return;

        _discovered = true;

        // Try assigned manager first, then fall back to singleton
        AreaDiscoveryManager mgr = manager != null ? manager : AreaDiscoveryManager.Instance;

        if (mgr != null)
            mgr.RegisterDiscovery(areaName);
        else
            Debug.LogWarning($"[DiscoverableArea] '{areaName}' could not find AreaDiscoveryManager.");
    }

    // ── Editor visualisation ──────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        if (col is SphereCollider sc)
            Gizmos.DrawWireSphere(transform.position, sc.radius);
        else
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
