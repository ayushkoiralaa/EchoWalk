using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EchoPoint : MonoBehaviour
{
    // ── Content ───────────────────────────────────────────────────────────────

    [Header("Location Content")]
    public string locationName = "Old Dharahara";

    [TextArea(4, 10)]
    public string storyText =
        "Built in 1832 by Prime Minister Bhimsen Thapa, the original Dharahara " +
        "stood 61.88 metres tall with nine storeys. It served as a watchtower " +
        "and a symbol of Nepal's strength. For 183 years it defined Kathmandu's " +
        "skyline — until April 25, 2015.";

    [Tooltip("Assign a historical image for locations that have one. Leave empty for text-only.")]
    public Sprite beforeImage;

    [Tooltip("Caption shown below the before-image, e.g. 'Dharahara, circa 1900'")]
    public string imageCaption = "";

    // ── Beam visuals ──────────────────────────────────────────────────────────

    [Header("Beam Settings")]
    public Color beamColor    = new Color(0.6f, 0.85f, 1f, 0.35f);
    public float beamHeight   = 6f;
    public float beamWidth    = 0.08f;
    public float pulseSpeed   = 1.2f;
    public float pulseMin     = 0.25f;
    public float pulseMax     = 0.45f;

    // ── Shared UI (assign same panel on all EchoPoints) ───────────────────────

    [Header("Story Panel UI (assign same refs on all EchoPoints)")]
    public GameObject      storyPanelRoot;
    public TextMeshProUGUI panelLocationName;
    public TextMeshProUGUI panelStoryText;
    public GameObject      imageSection;       // Parent holding image + caption
    public Image           panelBeforeImage;
    public TextMeshProUGUI panelImageCaption;
    public Button          panelCloseButton;

    [Header("Interaction Prompt")]
    public GameObject      promptRoot;         // Small "[E] Learn More" prompt
    public TextMeshProUGUI promptText;
    public float           promptRange = 5f;

    // ── Private ───────────────────────────────────────────────────────────────

    private GameObject   _beam;
    private Light        _pointLight;
    private Camera       _cam;
    private bool         _panelOpen;
    private float        _pulseTimer;

    private static EchoPoint _currentOpen;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _cam = Camera.main;

        BuildBeam();
        BuildPointLight();

        if (storyPanelRoot  != null) storyPanelRoot.SetActive(false);
        if (promptRoot      != null) promptRoot.SetActive(false);
        if (panelCloseButton!= null) panelCloseButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        PulseBeam();
        HandlePrompt();

        if (Input.GetKeyDown(KeyCode.E))
            TryOpen();
    }

    // ── Beam construction ─────────────────────────────────────────────────────

    void BuildBeam()
    {
        _beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _beam.name = "EchoBeam";

        // Remove collider — beam should not block movement
        Destroy(_beam.GetComponent<Collider>());

        // Position: base at ground, extend upward
        _beam.transform.SetParent(transform);
        _beam.transform.localPosition = new Vector3(0f, beamHeight * 0.5f, 0f);
        _beam.transform.localScale    = new Vector3(beamWidth, beamHeight * 0.5f, beamWidth);

        // Material
        Renderer r = _beam.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            // Fallback for non-URP projects
            mat = new Material(Shader.Find("Standard"));
        }

        mat.color = beamColor;

        // Enable transparency
        mat.SetFloat("_Surface", 1);          // URP transparent
        mat.SetFloat("_Mode", 3);             // Standard transparent
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = 3000;
        mat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");

        r.material        = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows  = false;
    }

    void BuildPointLight()
    {
        GameObject lightObj = new GameObject("EchoLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        _pointLight           = lightObj.AddComponent<Light>();
        _pointLight.type      = LightType.Point;
        _pointLight.color     = new Color(0.6f, 0.85f, 1f);
        _pointLight.intensity = 0.8f;
        _pointLight.range     = 4f;
    }

    // ── Pulse ─────────────────────────────────────────────────────────────────

    void PulseBeam()
    {
        if (_beam == null) return;

        _pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(_pulseTimer) + 1f) * 0.5f);

        Renderer r   = _beam.GetComponent<Renderer>();
        Color    col = r.material.color;
        col.a        = alpha;
        r.material.color = col;

        if (_pointLight != null)
            _pointLight.intensity = Mathf.Lerp(0.4f, 1f, (Mathf.Sin(_pulseTimer) + 1f) * 0.5f);
    }

    // ── Prompt ────────────────────────────────────────────────────────────────

    void HandlePrompt()
    {
        if (promptRoot == null || _cam == null || _panelOpen) return;

        // Show prompt when crosshair is aimed at the beam and player is close
        Ray  ray  = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool aimed = Physics.Raycast(ray, out RaycastHit hit, promptRange)
                     && hit.collider != null
                     && hit.collider.gameObject == gameObject;

        // Also show if player is very close even without aiming
        float dist = Vector3.Distance(_cam.transform.position, transform.position);
        bool  near = dist < promptRange * 0.5f;

        bool show = (aimed || near) && !_panelOpen;
        promptRoot.SetActive(show);

        if (show && promptText != null)
            promptText.text = $"[E]  Learn about {locationName}";
    }

    // ── Panel ─────────────────────────────────────────────────────────────────

    void TryOpen()
    {
        if (_panelOpen) return;

        float dist = Vector3.Distance(
            _cam != null ? _cam.transform.position : transform.position,
            transform.position);

        if (dist > promptRange) return;

        // Only open if prompt was showing (player is aimed at / near this point)
        if (promptRoot != null && !promptRoot.activeSelf) return;

        OpenPanel();
    }

    void OpenPanel()
    {
        if (storyPanelRoot == null) return;

        if (_currentOpen != null && _currentOpen != this)
            _currentOpen.ClosePanel();

        panelLocationName.text = locationName;
        panelStoryText.text    = storyText;

        // Show image section only if a before-image is assigned
        bool hasImage = beforeImage != null;
        if (imageSection    != null) imageSection.SetActive(hasImage);
        if (hasImage)
        {
            if (panelBeforeImage  != null) panelBeforeImage.sprite = beforeImage;
            if (panelImageCaption != null) panelImageCaption.text  = imageCaption;
        }

        storyPanelRoot.SetActive(true);
        if (promptRoot != null) promptRoot.SetActive(false);

        _panelOpen   = true;
        _currentOpen = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void ClosePanel()
    {
        if (storyPanelRoot != null) storyPanelRoot.SetActive(false);
        _panelOpen = false;
        if (_currentOpen == this) _currentOpen = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}
