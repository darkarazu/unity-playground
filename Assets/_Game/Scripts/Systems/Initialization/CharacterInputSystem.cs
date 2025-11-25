using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Components;

namespace Game.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class CharacterInputSystem : SystemBase
    {
        private InputActionAsset _inputActionAsset;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;

        protected override void OnCreate()
        {
            // Load the Input Action Asset. 
            // Note: In a real project, you might reference this via a ScriptableObject or Singleton.
            // For this implementation, we assume it's loaded or we create a default one if not found, 
            // but since we created the file, we can try to load it or just use direct action definitions for simplicity if the asset isn't automatically generated.
            // However, to be robust without the generated class, we will find the asset by name or path in a real scenario.
            // Here, we'll assume the user assigns it or we use a simple fallback. 
            // actually, without the generated class, we need to load the asset. 
            // For simplicity in this agentic context, let's assume a MonoBehaviour passes the input or we use the singleton pattern.
            // But to keep it self-contained, let's try to load it from Resources if possible, or just define actions manually here for the system.
            
            // BETTER APPROACH: Use a MonoBehaviour to initialize the InputSystem and store it in a singleton component, 
            // but for now, let's just create the actions directly since we know the bindings.
            
            // Create move action with proper 2D Vector composite for WASD
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");
                
            _jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");
            
            _sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
            _sprintAction.AddBinding("<Gamepad>/leftStickPress");
            
            _crouchAction = new InputAction("Crouch", binding: "<Keyboard>/leftCtrl");
            _crouchAction.AddBinding("<Gamepad>/buttonEast");

            _moveAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _crouchAction.Enable();
        }

        protected override void OnDestroy()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
            _sprintAction.Disable();
            _crouchAction.Disable();
        }

        protected override void OnUpdate()
        {
            var moveInput = _moveAction.ReadValue<Vector2>();
            var jumpPressed = _jumpAction.IsPressed();
            var sprintPressed = _sprintAction.IsPressed();
            var crouchPressed = _crouchAction.IsPressed();

            foreach (var inputData in SystemAPI.Query<RefRW<CharacterInputData>>())
            {
                inputData.ValueRW.MoveInput = moveInput;
                inputData.ValueRW.JumpPressed = jumpPressed;
                inputData.ValueRW.SprintPressed = sprintPressed;
                inputData.ValueRW.CrouchPressed = crouchPressed;
            }
        }
    }
}
