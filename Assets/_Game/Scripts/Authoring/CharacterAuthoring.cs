using Unity;
using UnityEngine;
using Unity.Physics;
using Unity.Entities;
using Game.Components;
using Unity.Mathematics;
using Unity.CharacterController;

namespace Game.Authoring
{
    public class CharacterAuthoring : MonoBehaviour
    {
        public float WalkSpeed = 5f;
        public float RunSpeed = 10f;
        public float CrouchSpeed = 2.5f;
        public float JumpHeight = 5f;
        public bool IsPlayerControlled = true;
        public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();

        public class CharacterBaker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new CharacterData
                {
                    WalkSpeed = authoring.WalkSpeed,
                    RunSpeed = authoring.RunSpeed,
                    CrouchSpeed = authoring.CrouchSpeed,
                    JumpHeight = authoring.JumpHeight
                });

                AddComponent(entity, new CharacterInputData());
                AddComponent(entity, new CharacterState());

                // Add PlayerTag if this is player-controlled
                if (authoring.IsPlayerControlled)
                {
                    AddComponent<PlayerTag>(entity);
                }

                // Add Kinematic Character Body components
                // BakeCharacter expects (Baker, GameObject, AuthoringKinematicCharacterProperties)
                KinematicCharacterUtilities.BakeCharacter(this, authoring.gameObject, authoring.CharacterProperties);
            }
        }
    }
}
