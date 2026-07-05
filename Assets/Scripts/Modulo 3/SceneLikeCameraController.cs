using UnityEngine;
using UnityEngine.InputSystem;

public class SceneLikeCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Vector3 target = Vector3.zero;

    [Header("Distance")]
    [SerializeField] private float distance = 80f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 1000f;

    [Header("Speed")]
    [SerializeField] private float orbitSpeed = 0.2f;
    [SerializeField] private float zoomSpeed = 0.12f;
    [SerializeField] private float panSpeed = 0.002f;

    [Header("Vertical Limits")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Options")]
    [SerializeField] private bool allowPan = false;
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 12f;

    private float yaw = 45f;
    private float pitch = 35f;

    private float targetDistance;
    private Vector3 currentTarget;
    private Vector3 targetTarget;

    private void Start()
    {
        targetDistance = distance;
        currentTarget = target;
        targetTarget = target;

        UpdateCameraPosition(true);
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        HandleOrbit();
        HandleZoom();
        HandlePan();
        HandleReset();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition(false);
    }

    private void HandleOrbit()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        bool rightClickOrbit = mouse.rightButton.isPressed;

        bool altLeftClickOrbit =
            keyboard != null &&
            (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed) &&
            mouse.leftButton.isPressed;

        if (!rightClickOrbit && !altLeftClickOrbit)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        yaw += mouseDelta.x * orbitSpeed;
        pitch -= mouseDelta.y * orbitSpeed;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleZoom()
    {
        Mouse mouse = Mouse.current;

        Vector2 scroll = mouse.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) < 0.01f)
            return;

        targetDistance -= scroll.y * zoomSpeed * targetDistance * Time.deltaTime;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void HandlePan()
    {
        if (!allowPan)
            return;

        Mouse mouse = Mouse.current;

        if (!mouse.middleButton.isPressed)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        Vector3 right = transform.right;
        Vector3 up = transform.up;

        Vector3 movement =
            (-right * mouseDelta.x - up * mouseDelta.y)
            * targetDistance
            * panSpeed;

        target += movement;
        targetTarget = target;
    }

    private void HandleReset()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (!keyboard.fKey.wasPressedThisFrame)
            return;

        target = Vector3.zero;
        targetTarget = target;

        targetDistance = 80f;
        yaw = 45f;
        pitch = 35f;
    }

    private void UpdateCameraPosition(bool instant)
    {
        if (smoothMovement && !instant)
        {
            distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * smoothSpeed);
            currentTarget = Vector3.Lerp(currentTarget, targetTarget, Time.deltaTime * smoothSpeed);
        }
        else
        {
            distance = targetDistance;
            currentTarget = targetTarget;
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 cameraPosition =
            currentTarget - rotation * Vector3.forward * distance;

        transform.position = cameraPosition;
        transform.rotation = rotation;
    }

    public void SetTarget(Vector3 newTarget)
    {
        Focus(newTarget);
    }

    public void LookAtOrigin()
    {
        SetTarget(Vector3.zero);
    }

    public void Focus(Vector3 newTarget)
    {
        target = newTarget;
        targetTarget = newTarget;
    }

    public void Focus(Vector3 newTarget, float newDistance)
    {
        target = newTarget;
        targetTarget = newTarget;

        targetDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    public void FocusInstant(Vector3 newTarget)
    {
        target = newTarget;
        targetTarget = newTarget;
        currentTarget = newTarget;

        UpdateCameraPosition(true);
    }
}