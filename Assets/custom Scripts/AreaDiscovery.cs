using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    private int _discovered = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (areaIndicatorText != null) areaIndicatorText.gameObject.SetActive(false);
        if (completionPopup != null) completionPopup.SetActive(false);
        if (completionCloseButton != null)
            completionCloseButton.onClick.AddListener(CloseCompletion);

        UpdateScoreLabel();
    }

    public void RegisterDiscovery(string areaName)
    {
        _discovered++;
        UpdateScoreLabel();
        ShowAreaIndicator(areaName);

        if (_discovered >= totalAreas)
            StartCoroutine(ShowCompletion());
    }

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
        areaIndicatorText.text = $"  {areaName}";
        areaIndicatorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(indicatorDuration);
        areaIndicatorText.gameObject.SetActive(false);
    }

    IEnumerator ShowCompletion()
    {
        yield return new WaitForSeconds(1.5f);

        if (completionPopup != null) completionPopup.SetActive(true);

        if (completionText != null)
            completionText.text =
     "\n\n\n\n\n<size=35>  You've explored all areas!</size>\n\n" +
     "Thank you for playing EchoWalk.\n\n" +
     "<size=35>Thank you for walking through\nthe story of Dharahara.\nIts legacy lives on.</size>";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseCompletion()
    {
        if (completionPopup != null) completionPopup.SetActive(false);
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
