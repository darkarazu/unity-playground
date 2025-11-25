using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Game.Player
{
    /// <summary>
    /// Controls the Cinemachine FreeLook camera with WoW-style controls.
    /// LMB: Orbit camera around player (view only)
    /// RMB: Rotate player to face camera direction
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class CameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [Tooltip("The Cinemachine FreeLook camera (drag from hierarchy)")]
        public CinemachineCamera freeLookCamera;

        [Header("Mouse Settings")]
        [Tooltip("Mouse sensitivity for camera rotation")]
        public float mouseSensitivity = 2f;

        [Tooltip("Invert vertical camera axis")]
        public bool invertY = false;

        [Tooltip("Minimum vertical angle (looking down)")]
        public float minVerticalAngle = 0f;

        [Tooltip("Maximum vertical angle (looking up)")]
        public float maxVerticalAngle = 90f;

        [Header("Zoom Settings")]
        [Tooltip("Zoom sensitivity (scroll wheel)")]
        public float zoomSensitivity = 5f;

        [Tooltip("Minimum zoom distance (closest)")]
        public float minZoom = 2f;

        [Tooltip("Maximum zoom distance (furthest)")]
        public float maxZoom = 8f;

        [Header("Player Rotation Settings")]
        [Tooltip("Speed of player rotation when RMB is held")]
        public float playerRotationSpeed = 10f;

        [Header("FOV Settings")]
        [Tooltip("Normal field of view")]
        public float normalFOV = 60f;

        [Tooltip("Field of view when sprinting")]
        public float sprintFOV = 70f;

        [Tooltip("Speed of FOV transition")]
        public float fovTransitionSpeed = 5f;

        // Input Actions
        private InputAction mousePosition;
        private InputAction leftMouseButton;
        private InputAction rightMouseButton;
        private InputAction scrollAction;

        // References
        private PlayerController playerController;
        private CinemachineOrbitalFollow orbitalFollow;

        // Camera state
        private Vector2 lastMousePosition;
        private float currentHorizontalAngle = 0f;
        private float currentVerticalAngle = 45f; // Middle of the rig range
        
        // Zoom state - store initial rig settings for proportional scaling
        private float initialTopRadius;
        private float initialTopHeight;
        private float initialCenterRadius;
        private float initialCenterHeight;
        private float initialBottomRadius;
        private float initialBottomHeight;
        private float currentZoomFactor = 1f; // 1.0 = initial size
        
        // FOV state
        private float currentFOV;
        private float targetFOV;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            SetupInputActions();
            FindCameraComponents();
        }

        private void SetupInputActions()
        {
            // Mouse position (delta for rotation)
            mousePosition = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/delta");
            mousePosition.Enable();

            // Left mouse button (orbit camera only)
            leftMouseButton = new InputAction("LeftMouse", InputActionType.Button, "<Mouse>/leftButton");
            leftMouseButton.Enable();

            // Right mouse button (rotate player + camera)
            rightMouseButton = new InputAction("RightMouse", InputActionType.Button, "<Mouse>/rightButton");
            rightMouseButton.Enable();

            // Mouse scroll (zoom)
            scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll/y");
            scrollAction.Enable();
        }

        private void FindCameraComponents()
        {
            if (freeLookCamera == null)
            {
                Debug.LogError("CameraController: FreeLook Camera reference not set! Drag the FreeLook Camera GameObject to this component.");
                enabled = false;
                return;
            }

            // Get the Orbital Follow component from the Cinemachine camera
            orbitalFollow = freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
            
            if (orbitalFollow == null)
            {
                Debug.LogError("CameraController: Cinemachine Orbital Follow component not found on camera!");
                enabled = false;
                return;
            }

            // Initialize current angles from the orbital follow
            currentHorizontalAngle = orbitalFollow.HorizontalAxis.Value;
            currentVerticalAngle = orbitalFollow.VerticalAxis.Value;
            
            // Store initial orbital rig values for proportional zooming
            var orbits = orbitalFollow.Orbits;
            initialTopRadius = orbits.Top.Radius;
            initialTopHeight = orbits.Top.Height;
            initialCenterRadius = orbits.Center.Radius;
            initialCenterHeight = orbits.Center.Height;
            initialBottomRadius = orbits.Bottom.Radius;
            initialBottomHeight = orbits.Bottom.Height;
            
            // Initialize FOV
            currentFOV = normalFOV;
            targetFOV = normalFOV;
            freeLookCamera.Lens.FieldOfView = normalFOV;
        }

        private void LateUpdate()
        {
            HandleCameraInput();
            HandleZoom();
            HandleFOV();
        }

        private void HandleCameraInput()
        {
            // Get mouse delta
            Vector2 mouseDelta = mousePosition.ReadValue<Vector2>();
            
            bool isLeftMouseHeld = leftMouseButton.IsPressed();
            bool isRightMouseHeld = rightMouseButton.IsPressed();

            // Only process if either mouse button is held
            if (isLeftMouseHeld || isRightMouseHeld)
            {
                // Apply mouse sensitivity
                float deltaX = mouseDelta.x * mouseSensitivity * 0.01f;
                float deltaY = mouseDelta.y * mouseSensitivity * 0.01f;

                // Invert Y if needed
                if (invertY)
                    deltaY = -deltaY;

                // Update horizontal axis (Left/Right rotation)
                currentHorizontalAngle += deltaX;

                // Update vertical axis (Up/Down) - clamped to rig range
                currentVerticalAngle -= deltaY; // Subtract because mouse up should move camera up
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

                // Apply to Cinemachine camera
                orbitalFollow.HorizontalAxis.Value = currentHorizontalAngle;
                orbitalFollow.VerticalAxis.Value = currentVerticalAngle;

                // If RIGHT mouse button: Rotate player to face camera direction
                if (isRightMouseHeld)
                {
                    RotatePlayerToCameraDirection();
                }
            }
        }

        private void RotatePlayerToCameraDirection()
        {
            // Get the camera's horizontal rotation (Y-axis only)
            float cameraYRotation = freeLookCamera.transform.eulerAngles.y;

            // Create target rotation (only Y-axis, keep X and Z at 0)
            Quaternion targetRotation = Quaternion.Euler(0f, cameraYRotation, 0f);
            
            // Smoothly rotate player towards camera direction
            Quaternion currentRotation = playerController.transform.rotation;
            Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * playerRotationSpeed);
            
            // Apply smooth rotation
            playerController.SetYRotation(newRotation.eulerAngles.y);
        }

        private void HandleZoom()
        {
            float scrollValue = scrollAction.ReadValue<float>();
            
            if (scrollValue != 0 && orbitalFollow != null)
            {
                // Adjust zoom factor based on scroll input
                // Positive scroll = zoom in (smaller factor), negative = zoom out (larger factor)
                float zoomChange = scrollValue * zoomSensitivity * 0.01f;
                currentZoomFactor = Mathf.Clamp(currentZoomFactor - zoomChange, 
                    minZoom / initialCenterRadius,  // Min zoom as ratio of initial center radius
                    maxZoom / initialCenterRadius); // Max zoom as ratio of initial center radius
                
                // Get current orbit settings
                var orbits = orbitalFollow.Orbits;
                
                // Scale all rig dimensions proportionally based on zoom factor
                // This maintains the original shape and proportions of the orbital sphere
                orbits.Top.Radius = initialTopRadius * currentZoomFactor;
                orbits.Top.Height = initialTopHeight * currentZoomFactor;
                
                orbits.Center.Radius = initialCenterRadius * currentZoomFactor;
                orbits.Center.Height = initialCenterHeight * currentZoomFactor;
                
                orbits.Bottom.Radius = initialBottomRadius * currentZoomFactor;
                orbits.Bottom.Height = initialBottomHeight * currentZoomFactor;
                
                // Apply back to orbital follow
                orbitalFollow.Orbits = orbits;
            }
        }

        private void HandleFOV()
        {
            if (freeLookCamera == null || playerController == null)
                return;

            // Set target FOV based on sprint state AND actual movement
            // Only widen FOV if player is sprinting AND moving
            bool shouldUseSprintFOV = playerController.IsSprinting && playerController.IsMoving;
            targetFOV = shouldUseSprintFOV ? sprintFOV : normalFOV;

            // Smoothly transition to target FOV
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);

            // Apply to camera
            freeLookCamera.Lens.FieldOfView = currentFOV;
        }

        private void OnDestroy()
        {
            // Cleanup input actions
            mousePosition?.Disable();
            leftMouseButton?.Disable();
            rightMouseButton?.Disable();
            scrollAction?.Disable();
        }

        // Public method to get camera forward direction (for player movement)
        public Vector3 GetCameraForward()
        {
            if (freeLookCamera != null)
            {
                Vector3 forward = freeLookCamera.transform.forward;
                forward.y = 0; // Flatten to horizontal plane
                return forward.normalized;
            }
            return Vector3.forward;
        }

        public Vector3 GetCameraRight()
        {
            if (freeLookCamera != null)
            {
                Vector3 right = freeLookCamera.transform.right;
                right.y = 0; // Flatten to horizontal plane
                return right.normalized;
            }
            return Vector3.right;
        }
    }
}
