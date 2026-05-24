using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // ── Audio clips ───────────────────────────────────────────────────────────

    [Header("Start Screen Music")]
    [Tooltip("Ambient music played on the start screen.")]
    public AudioClip startMusic;

    [System.Serializable]
    public class AreaTrack
    {
        [Tooltip("Must match exactly the Area Name in DiscoverableArea, e.g. 'Old Dharahara'")]
        public string   areaName;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float    volume = 0.6f;
    }

    [Header("Area Music")]
    public AreaTrack[] areaTracks;

    [Header("Crossfade")]
    [Tooltip("Seconds to fade between tracks.")]
    public float crossfadeDuration = 1.5f;

    // ── UI ────────────────────────────────────────────────────────────────────

    [Header("Music Toggle UI")]
    [Tooltip("Small TMP label that briefly shows 'Music On' / 'Music Off' when M is pressed.")]
    public TextMeshProUGUI musicToggleIndicator;
    public float           indicatorDuration = 2f;

    // ── Private ───────────────────────────────────────────────────────────────

    private AudioSource _sourceA;   // Two sources for crossfading
    private AudioSource _sourceB;
    private bool        _usingA    = true;
    private bool        _musicOn   = true;
    private string      _currentArea;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();

        _sourceA.loop       = true;
        _sourceB.loop       = true;
        _sourceA.playOnAwake = false;
        _sourceB.playOnAwake = false;

        // Important: these are MUSIC sources only
        // Footstep and jump AudioSources on the player are separate
        // and are NOT affected by the M key toggle here
    }

    void Start()
    {
        if (musicToggleIndicator != null)
            musicToggleIndicator.gameObject.SetActive(false);

        // Play start screen music immediately
        if (startMusic != null)
            PlayClip(startMusic, 0.5f);
    }

    void Update()
    {
        // M key toggles music only
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMusic();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by AreaMusicTrigger when player enters an area.</summary>
    public void PlayAreaMusic(string areaName)
    {
        if (areaName == _currentArea) return;
        _currentArea = areaName;

        foreach (AreaTrack track in areaTracks)
        {
            if (track.areaName == areaName && track.clip != null)
            {
                StartCoroutine(Crossfade(track.clip, track.volume));
                return;
            }
        }
    }

    /// <summary>Call this when start button is clicked to transition from start music.</summary>
    public void OnGameStart()
    {
        // Fade out start music — area music takes over when player enters a zone
        StartCoroutine(FadeOut(GetActiveSource(), crossfadeDuration));
    }

    // ── Music toggle ──────────────────────────────────────────────────────────

    void ToggleMusic()
    {
        _musicOn = !_musicOn;

        _sourceA.mute = !_musicOn;
        _sourceB.mute = !_musicOn;

        StopCoroutine(nameof(ShowToggleIndicator));
        StartCoroutine(nameof(ShowToggleIndicator));
    }

    IEnumerator ShowToggleIndicator()
    {
        if (musicToggleIndicator == null) yield break;

        musicToggleIndicator.text = _musicOn ? "♪  Music On" : "♪  Music Off";
        musicToggleIndicator.gameObject.SetActive(true);

        yield return new WaitForSeconds(indicatorDuration);

        musicToggleIndicator.gameObject.SetActive(false);
    }

    // ── Playback helpers ──────────────────────────────────────────────────────

    void PlayClip(AudioClip clip, float volume)
    {
        AudioSource active = GetActiveSource();
        active.clip   = clip;
        active.volume = _musicOn ? volume : 0f;
        active.mute   = !_musicOn;
        active.Play();
    }

    IEnumerator Crossfade(AudioClip newClip, float targetVolume)
    {
        AudioSource outgoing = GetActiveSource();
        AudioSource incoming = GetInactiveSource();

        _usingA = !_usingA;

        incoming.clip   = newClip;
        incoming.volume = 0f;
        incoming.mute   = !_musicOn;
        incoming.Play();

        float elapsed = 0f;
        float startVol = outgoing.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed        += Time.deltaTime;
            float t         = elapsed / crossfadeDuration;
            outgoing.volume = Mathf.Lerp(startVol,   0f,           t);
            incoming.volume = Mathf.Lerp(0f,          targetVolume, t);
            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        incoming.volume = targetVolume;
    }

    IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed       += Time.deltaTime;
            source.volume  = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
    }

    AudioSource GetActiveSource()   => _usingA ? _sourceA : _sourceB;
    AudioSource GetInactiveSource() => _usingA ? _sourceB : _sourceA;
}
