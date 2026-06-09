using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public sealed class SimpleFirstPersonWalkController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 3.4f;
    [SerializeField] private float mouseSensitivity = 1.4f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private bool movementEnabled = true;

    private CharacterController characterController;
    private Vector3 defaultCameraLocalPosition;
    private float pitch;
    private float verticalVelocity;
    private bool uiFocusActive;

    public Camera PlayerCamera => playerCamera;
    public bool MovementEnabled => movementEnabled;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera != null)
        {
            defaultCameraLocalPosition = playerCamera.transform.localPosition;
        }
    }

    private void OnEnable()
    {
        RefreshCursorState();
    }

    private void Update()
    {
        if (!movementEnabled || uiFocusActive)
        {
            return;
        }

        Look();
        Move();
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        RefreshCursorState();
    }

    public void SetUiFocusActive(bool active)
    {
        if (uiFocusActive == active)
        {
            return;
        }

        uiFocusActive = active;
        RefreshCursorState();
    }

    public void ConfigureCamera(Camera camera)
    {
        playerCamera = camera;
        if (playerCamera != null)
        {
            defaultCameraLocalPosition = playerCamera.transform.localPosition;
        }
    }

    public void WarpTo(Vector3 position, Quaternion rotation)
    {
        bool wasEnabled = characterController.enabled;
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));
        characterController.enabled = wasEnabled;

        pitch = NormalizePitch(rotation.eulerAngles.x);
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = defaultCameraLocalPosition;
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void Move()
    {
        Vector2 input = GetMoveInput();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move = Vector3.ClampMagnitude(move, 1f) * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);
    }

    private void Look()
    {
        Vector2 mouseDelta = GetMouseDelta() * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseDelta.x);
        pitch = Mathf.Clamp(pitch - mouseDelta.y, -78f, 78f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private static Vector2 GetMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;
        input.x += Keyboard.current.dKey.isPressed ? 1f : 0f;
        input.x -= Keyboard.current.aKey.isPressed ? 1f : 0f;
        input.y += Keyboard.current.wKey.isPressed ? 1f : 0f;
        input.y -= Keyboard.current.sKey.isPressed ? 1f : 0f;
        return input;
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    private static Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue();
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void RefreshCursorState()
    {
        SetCursorLocked(movementEnabled && !uiFocusActive);
    }

    private static float NormalizePitch(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
