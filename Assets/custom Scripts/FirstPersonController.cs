
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // ── Inspector fields ──────────────────────────────────────────────────────

    [Header("Movement")]
    public float walkSpeed   = 4f;
    public float sprintSpeed = 8f;
    public float gravity     = -20f;   // stronger gravity 
    public float jumpHeight  = 1.2f;

    [Header("Mouse Look")]
    public Camera mainCamera;           
    public float  mouseSensitivity = 2f;
    public float  pitchMin = -80f;
    public float  pitchMax =  80f;

    [Header("Interaction")]
    public float           interactionRange = 3f;
    public KeyCode         interactKey      = KeyCode.E;
    public GameObject      interactionPromptUI;   // Parent GameObject of the HUD prompt
    public TextMeshProUGUI interactionPromptText;  // The TMP label inside it
    public LayerMask       interactableLayer;      // Layer your hotspots/NPC are on

    [Header("Head Bob (optional)")]
    public bool  enableHeadBob = true;
    public float bobFrequency  = 10f;
    public float bobAmplitude  = 0.05f;

    [Header("Animation (optional)")]
    public Animator playerAnimator;
    public string   speedParam      = "Speed";
    public string   isGroundedParam = "IsGrounded";
    public string   jumpTrigParam   = "Jump";

    // ── Private state ─────────────────────────────────────────────────────────

    private CharacterController _cc;
    private Vector3             _velocity;        // tracks Y (gravity / jump)
    private float               _pitch;           // camera up/down tilt
    private float               _bobTimer;
    private Vector3             _camRestPos;      // camera local position at rest
    private IInteractable       _currentInteractable;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            _camRestPos = mainCamera.transform.localPosition;
    }

    void Start()
    {
        LockCursor();

        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }

    void Update()
    {
        HandleCursorToggle();
        HandleMouseLook();
        HandleGravityAndJump();
        HandleMovement();
        HandleHeadBob();
        HandleInteraction();
    }

    // ── Cursor lock ───────────────────────────────────────────────────────────

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();

        // Re-lock when clicking anywhere that is NOT a UI element
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            LockCursor();
    }

    // ── Mouse look ────────────────────────────────────────────────────────────
    // Player body rotates left/right (yaw).
    // Camera child tilts up/down (pitch), clamped to prevent flipping.

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw — rotate entire player body
        transform.Rotate(Vector3.up, mouseX, Space.World);

        // Pitch — tilt camera only
        _pitch = Mathf.Clamp(_pitch - mouseY, pitchMin, pitchMax);
        if (mainCamera != null)
            mainCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    // ── Gravity & jump ────────────────────────────────────────────────────────

    void HandleGravityAndJump()
    {
        bool grounded = _cc.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -4f;   // keeps the controller firmly on the ground

        if (Input.GetButtonDown("Jump") && grounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (playerAnimator != null) playerAnimator.SetTrigger(jumpTrigParam);
        }

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);

        if (playerAnimator != null)
            playerAnimator.SetBool(isGroundedParam, grounded);
    }

    // ── WASD movement ─────────────────────────────────────────────────────────

    void HandleMovement()
    {
        float h    = Input.GetAxisRaw("Horizontal");
        float v    = Input.GetAxisRaw("Vertical");
        float spd  = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Move relative to the player's facing direction (yaw only — no pitch tilt)
        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        _cc.Move(move * spd * Time.deltaTime);

        if (playerAnimator != null)
            playerAnimator.SetFloat(speedParam, move.magnitude * spd);
    }

    // ── Head bob ──────────────────────────────────────────────────────────────

    void HandleHeadBob()
    {
        if (!enableHeadBob || mainCamera == null) return;

        bool moving = _cc.isGrounded &&
                      (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.05f ||
                       Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0.05f);

        if (moving)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            mainCamera.transform.localPosition = _camRestPos + new Vector3(
                Mathf.Cos(_bobTimer * 0.5f) * bobAmplitude * 0.5f,
                Mathf.Sin(_bobTimer)        * bobAmplitude,
                0f);
        }
        else
        {
            _bobTimer = 0f;
            mainCamera.transform.localPosition = Vector3.Lerp(
                mainCamera.transform.localPosition, _camRestPos, Time.deltaTime * 10f);
        }
    }

    // ── Interaction ───────────────────────────────────────────────────────────
    // Casts a ray from the centre of the screen (where the FPS crosshair is).
    // Falls back to a proximity sphere for objects slightly off-centre.

    void HandleInteraction()
    {
        if (mainCamera == null) return;

        IInteractable found = null;

        // Primary: crosshair raycast
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
            found = hit.collider.GetComponent<IInteractable>();

        // Fallback: small proximity sphere around the player
        if (found == null)
        {
            Collider[] cols = Physics.OverlapSphere(
                transform.position, interactionRange * 0.5f, interactableLayer);
            foreach (Collider col in cols)
            {
                IInteractable c = col.GetComponent<IInteractable>();
                if (c != null) { found = c; break; }
            }
        }

        _currentInteractable = found;

        // Show/hide HUD prompt
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(found != null);

        if (found != null && interactionPromptText != null)
            interactionPromptText.text = $"[E]  {found.GetPromptText()}";

        // Fire interaction
        if (Input.GetKeyDown(interactKey) && found != null)
            found.Interact(gameObject);
    }

    // ── Editor helpers ────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Proximity sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange * 0.5f);

        // Interaction ray
        if (mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(mainCamera.transform.position,
                           mainCamera.transform.forward * interactionRange);
        }
    }
}

// ── IInteractable interface ───────────────────────────────────────────────────
// Any object the player can interact with must implement this.
// If you put it in its own file (IInteractable.cs), remove it here.

public interface IInteractable
{
    /// <summary>Short text shown in the HUD prompt, e.g. "Inspect: North Pillar"</summary>
    string GetPromptText();

    /// <summary>Called when the player presses [E] while looking at this object.</summary>
    void Interact(GameObject interactor);
}
