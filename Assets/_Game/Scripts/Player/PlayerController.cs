using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// MonoBehaviour-based player controller using Unity's built-in Character Controller.
    /// Handles input, movement, jumping, sprinting, and crouching.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Walking speed in units per second")]
        public float walkSpeed = 5f;

        [Tooltip("Running speed when sprinting")]
        public float runSpeed = 10f;

        [Tooltip("Crouching speed")]
        public float crouchSpeed = 2.5f;

        [Header("Jump Settings")]
        [Tooltip("Jump velocity")]
        public float jumpHeight = 5f;

        [Tooltip("Gravity applied to the player")]
        public float gravity = 20f;

        [Header("Ground Detection")]
        [Tooltip("Distance to check for ground")]
        public float groundCheckDistance = 0.2f;

        [Tooltip("Layer mask for ground detection")]
        public LayerMask groundMask = 1; // Default layer

        // Components
        private CharacterController characterController;
        private CameraController cameraController;
        
        // Input
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;

        // Movement state
        private Vector3 velocity;
        private bool isGrounded;
        private bool isSprinting;
        private bool isCrouching;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cameraController = GetComponent<CameraController>();
            SetupInput();
        }

        private void SetupInput()
        {
            // Create input actions programmatically
            // Movement: WASD (Keyboard) or Left Stick (Gamepad)
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");

            // Jump: Space (Keyboard) or B/Cross/B (Gamepad buttonSouth)
            // buttonSouth = Xbox A, PS Cross, Switch B
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            // Sprint: Left Shift (Keyboard) or Left Trigger (Gamepad)
            // leftTrigger = Xbox LT, PS L2, Switch ZL
            sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
            sprintAction.AddBinding("<Gamepad>/leftTrigger");

            // Crouch: Left Control (Keyboard) or Left Shoulder (Gamepad)
            // leftShoulder = Xbox LB, PS L1, Switch L
            crouchAction = new InputAction("Crouch", binding: "<Keyboard>/leftCtrl");
            crouchAction.AddBinding("<Gamepad>/leftShoulder");

            // Enable actions
            moveAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            crouchAction.Enable();
        }

        private void OnDestroy()
        {
            // Disable actions
            moveAction?.Disable();
            jumpAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleJump();
        }

        private void HandleGroundCheck()
        {
            // Check if grounded using raycast slightly below the character
            Vector3 rayOrigin = transform.position;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, 
                characterController.height / 2f + groundCheckDistance, groundMask);
        }

        private void HandleMovement()
        {
            // Read input
            Vector2 input = moveAction.ReadValue<Vector2>();
            isSprinting = sprintAction.IsPressed();
            isCrouching = crouchAction.IsPressed();

            // Calculate speed based on state
            float currentSpeed = walkSpeed;
            if (isSprinting && !isCrouching)
                currentSpeed = runSpeed;
            else if (isCrouching)
                currentSpeed = crouchSpeed;

            // Calculate movement direction relative to player facing (WoW-style strafe)
            // W/S = forward/backward relative to player
            // A/D = strafe left/right relative to player
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 move = (forward * input.y) + (right * input.x);

            // Apply movement
            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Apply gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }

            velocity.y -= gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Player rotation is now controlled by CameraController (RMB)
            // No automatic rotation toward movement direction
        }

        private void HandleJump()
        {
            if (jumpAction.WasPressedThisFrame() && isGrounded)
            {
                velocity.y = jumpHeight;
            }
        }

        /// <summary>
        /// Sets the player's Y rotation (called by CameraController when RMB held)
        /// </summary>
        public void SetYRotation(float yAngle)
        {
            transform.rotation = Quaternion.Euler(0, yAngle, 0);
        }

        // Public getters for debugging/external systems
        public bool IsGrounded => isGrounded;
        public bool IsSprinting => isSprinting;
        public bool IsCrouching => isCrouching;
        public Vector3 Velocity => velocity;
        
        // Check if player has horizontal movement input (more reliable than velocity on terrain)
        public bool IsMoving => moveAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;
    }
}
