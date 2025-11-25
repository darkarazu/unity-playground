using Unity.Entities;

namespace Game.Components
{
    public struct CharacterData : IComponentData
    {
        public float WalkSpeed;
        public float RunSpeed;
        public float CrouchSpeed;
        public float JumpHeight;
    }
}
