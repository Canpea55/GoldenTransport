using UnityEngine;
using Unity.Cinemachine;
using Object = UnityEngine.Object;

[DisallowMultipleComponent]
public class DragInputController : InputAxisControllerBase<DragInputController.DragReader>
{
    [Header("Cinemachine (Cinemachine 3.x)")]
    [Tooltip("Assign your Cinemachine 3.x camera component here.")]
    public CinemachineCamera cinemachineCamera;

    [Header("Zoom (mouse wheel / pinch)")]
    public float zoomSpeed = 5f;
    public float minFov = 15f;
    public float maxFov = 30f;

    // tracked focal length
    private float currentFocal;

    void Start()
    {
        if (cinemachineCamera != null)
        {
            // Lens is a struct: copy, read, clamp
            var lens = cinemachineCamera.Lens;
            currentFocal = Mathf.Clamp(lens.FieldOfView, minFov, maxFov);
        }
        else
        {
            Debug.LogWarning("DragInputController: assign a CinemachineCamera in the inspector.");
            currentFocal = Mathf.Clamp(60f, minFov, maxFov);
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        HandleZoomInput();
        UpdateControllers(); // required to update InputAxisControllerBase
    }

    void HandleZoomInput()
    {
        float scrollDelta = 0f;

        // Mouse wheel
        scrollDelta -= Input.mouseScrollDelta.y;

        // Touch pinch (legacy Input API)
        if (Input.touchCount >= 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevPos0 = t0.position - t0.deltaPosition;
            Vector2 prevPos1 = t1.position - t1.deltaPosition;

            float prevDistance = Vector2.Distance(prevPos0, prevPos1);
            float curDistance = Vector2.Distance(t0.position, t1.position);
            float delta = curDistance - prevDistance;

            // scale down to keep similar feel to mouse wheel
            scrollDelta += delta * 0.1f;
        }

        if (Mathf.Approximately(scrollDelta, 0f) || cinemachineCamera == null)
            return;

        // Positive scrollDelta -> increase focal length => zoom in (telephoto)
        currentFocal += scrollDelta * zoomSpeed;
        currentFocal = Mathf.Clamp(currentFocal, minFov, maxFov);

        ApplyFocal(currentFocal);
    }

    void ApplyFocal(float focal)
    {
        if (cinemachineCamera == null) return;

        // Lens is a struct on CinemachineCamera: copy, modify, assign back
        var lens = cinemachineCamera.Lens;
        lens.FieldOfView = focal;
        cinemachineCamera.Lens = lens;
    }

    [System.Serializable]
    public class DragReader : IInputAxisReader
    {
        [Tooltip("Multiplier for horizontal mouse/touch delta")]
        public float horizontalSensitivity = 0.2f;

        [Tooltip("Multiplier for vertical mouse/touch delta")]
        public float verticalSensitivity = 0.1f;

        // Legacy input helpers for mouse dragging
        private Vector2 lastMousePos;
        private bool draggingMouse = false;

        public float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            Vector2 delta = Vector2.zero;

            // Mouse dragging
            if (Input.GetMouseButtonDown(0))
            {
                draggingMouse = true;
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                draggingMouse = false;
            }

            if (draggingMouse)
            {
                Vector2 current = Input.mousePosition;
                delta = current - lastMousePos;
                lastMousePos = current;
            }
            else if (Input.touchCount == 1)
            {
                // Single touch drag
                Touch t = Input.GetTouch(0);
                delta = t.deltaPosition;
            }
            else
            {
                return 0f;
            }

            // Convert delta into axis float based on hint
            switch (hint)
            {
                case IInputAxisOwner.AxisDescriptor.Hints.X:
                    return delta.x * horizontalSensitivity;
                case IInputAxisOwner.AxisDescriptor.Hints.Y:
                    return -delta.y * verticalSensitivity; // negate Y for typical drag-to-look
                default:
                    return delta.x * horizontalSensitivity;
            }
        }
    }
}
