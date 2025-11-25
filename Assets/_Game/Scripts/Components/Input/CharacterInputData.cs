using Unity.Entities;
using Unity.Mathematics;

namespace Game.Components
{
    public struct CharacterInputData : IComponentData
    {
        public float2 MoveInput;
        public bool JumpPressed;
        public bool SprintPressed;
        public bool CrouchPressed;
    }
}
