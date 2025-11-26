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

        [Tooltip("Joystick sensitivity for camera rotation (controller right stick)")]
        public float joystickSensitivity = 100f;

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
        private InputAction rightStickInput;  // Gamepad right stick for camera
        private InputAction dpadZoomIn;       // Gamepad D-Pad up for zoom in
        private InputAction dpadZoomOut;      // Gamepad D-Pad down for zoom out

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
        
        // Rotation catch-up state
        private bool shouldCatchUpRotation = false;
        private float rotationCatchUpTimer = 0f;
        private const float rotationCatchUpDuration = 0.3f; // Time to finish rotation after releasing input

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

            // Mouse scroll (zoom) + D-Pad support
            scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll/y");
            scrollAction.Enable();
            
            // Controller: Right Stick for camera orbit (replaces mouse delta when used)
            // rightStick = Xbox Right Stick, PS R3, Switch Right Stick
            rightStickInput = new InputAction("RightStick", InputActionType.Value, "<Gamepad>/rightStick");
            rightStickInput.Enable();
            
            // Controller: D-Pad for zoom (separate buttons)
            dpadZoomIn = new InputAction("DPadZoomIn", InputActionType.Button, "<Gamepad>/dpad/up");
            dpadZoomIn.Enable();
            
            dpadZoomOut = new InputAction("DPadZoomOut", InputActionType.Button, "<Gamepad>/dpad/down");
            dpadZoomOut.Enable();
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
            HandleRotationCatchUp();
        }

        private void HandleCameraInput()
        {
            // Get mouse delta
            Vector2 mouseDelta = mousePosition.ReadValue<Vector2>();
            
            // Get right stick input for gamepad camera control
            Vector2 rightStick = rightStickInput.ReadValue<Vector2>();
            
            bool isLeftMouseHeld = leftMouseButton.IsPressed();
            bool isRightMouseHeld = rightMouseButton.IsPressed();
            bool isRightStickUsed = rightStick.sqrMagnitude > 0.1f;

            // Process if mouse button held OR right stick moved (gamepad)
            if (isLeftMouseHeld || isRightMouseHeld || isRightStickUsed)
            {
                float deltaX, deltaY;
                
                if (isRightStickUsed)
                {
                    // Use joystick sensitivity for controller
                    deltaX = rightStick.x * joystickSensitivity * Time.deltaTime;
                    deltaY = rightStick.y * joystickSensitivity * Time.deltaTime;
                }
                else
                {
                    // Use mouse sensitivity for mouse
                    deltaX = mouseDelta.x * mouseSensitivity * 0.01f;
                    deltaY = mouseDelta.y * mouseSensitivity * 0.01f;
                }

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

                // If RIGHT mouse OR right stick: Rotate player immediately AND prepare catch-up
                if (isRightMouseHeld || isRightStickUsed)
                {
                    RotatePlayerToCameraDirection();
                    
                    // Enable rotation catch-up for when input is released
                    shouldCatchUpRotation = true;
                    rotationCatchUpTimer = rotationCatchUpDuration; // Reset timer
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
        
        private void HandleRotationCatchUp()
        {
            // Continue rotating player to match camera after RMB/right stick is released
            if (shouldCatchUpRotation && rotationCatchUpTimer > 0f)
            {
                // Check if RMB or right stick is still being used
                bool isRightMouseHeld = rightMouseButton.IsPressed();
                Vector2 rightStick = rightStickInput.ReadValue<Vector2>();
                bool isRightStickUsed = rightStick.sqrMagnitude > 0.1f;
                
                // If input resumed, reset timer
                if (isRightMouseHeld || isRightStickUsed)
                {
                    rotationCatchUpTimer = rotationCatchUpDuration;
                    return;
                }
                
                // Continue rotating toward camera
                RotatePlayerToCameraDirection();
                
                // Countdown timer
                rotationCatchUpTimer -= Time.deltaTime;
                
                // Stop when timer expires
                if (rotationCatchUpTimer <= 0f)
                {
                    shouldCatchUpRotation = false;
                }
            }
        }

        private void HandleZoom()
        {
            float scrollValue = scrollAction.ReadValue<float>();
            
            // Get D-Pad input for gamepad zoom (separate buttons, not a vector)
            float dpadUp = dpadZoomIn.ReadValue<float>();      // 1.0 when pressed, 0.0 otherwise
            float dpadDown = dpadZoomOut.ReadValue<float>();   // 1.0 when pressed, 0.0 otherwise
            float dpadZoomValue = dpadUp - dpadDown;           // +1 for zoom in, -1 for zoom out
            
            // Combine scroll and D-Pad
            // D-Pad: Progressive zoom based on hold duration (multiplied by deltaTime)
            // Scroll: Instant zoom based on scroll amount
            float zoomInput;
            if (Mathf.Abs(dpadZoomValue) > 0.1f)
            {
                // D-Pad held: apply progressive zoom (smooth increment per frame)
                zoomInput = dpadZoomValue * 200f * Time.deltaTime; // Adjust 200f to change D-Pad zoom speed
            }
            else
            {
                // Mouse scroll
                zoomInput = scrollValue;
            }
            
            if (zoomInput != 0 && orbitalFollow != null)
            {
                // Adjust zoom factor based on scroll input
                // Positive scroll = zoom in (smaller factor), negative = zoom out (larger factor)
                float zoomChange = zoomInput * zoomSensitivity * 0.01f;
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
            rightStickInput?.Disable();
            dpadZoomIn?.Disable();
            dpadZoomOut?.Disable();
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
