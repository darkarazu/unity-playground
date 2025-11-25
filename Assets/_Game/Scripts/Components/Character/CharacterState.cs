using Unity.Entities;

namespace Game.Components
{
    public struct CharacterState : IComponentData
    {
        public bool IsGrounded;
        public bool IsSprinting;
        public bool IsCrouching;
        public bool IsJumping;
    }
}
