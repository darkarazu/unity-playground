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
            SetupInput();
        }

        private void SetupInput()
        {
            // Create input actions programmatically
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");

            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
            sprintAction.AddBinding("<Gamepad>/leftStickPress");

            crouchAction = new InputAction("Crouch", binding: "<Keyboard>/leftCtrl");
            crouchAction.AddBinding("<Gamepad>/buttonEast");

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

            // Calculate movement direction (world space)
            Vector3 move = new Vector3(input.x, 0, input.y);

            // Apply movement
            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Apply gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }

            velocity.y -= gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Rotate player to face movement direction
            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void HandleJump()
        {
            if (jumpAction.WasPressedThisFrame() && isGrounded)
            {
                velocity.y = jumpHeight;
            }
        }

        // Public getters for debugging/external systems
        public bool IsGrounded => isGrounded;
        public bool IsSprinting => isSprinting;
        public bool IsCrouching => isCrouching;
        public Vector3 Velocity => velocity;
    }
}
