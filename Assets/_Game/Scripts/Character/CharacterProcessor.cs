using Unity.Entities;
using Unity.Mathematics;
using Unity.CharacterController;
using Game.Components;

namespace Game.Character
{
    public struct CharacterUpdateContext
    {
        public void OnSystemCreate(ref SystemState state)
        {
        }

        public void OnSystemUpdate(ref SystemState state)
        {
        }
    }

    public struct CharacterProcessor : IKinematicCharacterProcessor<CharacterUpdateContext>
    {
        public KinematicCharacterDataAccess CharacterDataAccess;
        public RefRW<CharacterData> CharacterData;
        public RefRW<CharacterInputData> CharacterInputData;
        public RefRW<CharacterState> CharacterState;

        public void PhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            ref KinematicCharacterBody characterBody = ref CharacterDataAccess.CharacterBody.ValueRW;
            ref float3 characterPosition = ref CharacterDataAccess.LocalTransform.ValueRW.Position;

            // Initialize character update
            KinematicCharacterUtilities.Update_Initialize(
                in this,
                ref context,
                ref baseContext,
                ref characterBody,
                CharacterDataAccess.CharacterHitsBuffer,
                CharacterDataAccess.DeferredImpulsesBuffer,
                CharacterDataAccess.VelocityProjectionHits,
                baseContext.Time.DeltaTime);

            // Handle grounding
            KinematicCharacterUtilities.Update_Grounding(
                in this,
                ref context,
                ref baseContext,
                ref characterBody,
                CharacterDataAccess.CharacterEntity,
                CharacterDataAccess.CharacterProperties.ValueRO,
                CharacterDataAccess.PhysicsCollider.ValueRO,
                CharacterDataAccess.LocalTransform.ValueRO,
                CharacterDataAccess.VelocityProjectionHits,
                CharacterDataAccess.CharacterHitsBuffer,
                ref characterPosition);

            // Update velocity based on input (custom logic)
            HandleVelocityControl(ref context, ref baseContext);

            // Apply movement and resolve collisions
            KinematicCharacterUtilities.Update_MovementAndDecollisions(
                in this,
                ref context,
                ref baseContext,
                CharacterDataAccess.CharacterEntity,
                ref characterBody,
                CharacterDataAccess.CharacterProperties.ValueRO,
                CharacterDataAccess.PhysicsCollider.ValueRO,
                CharacterDataAccess.LocalTransform.ValueRO,
                CharacterDataAccess.VelocityProjectionHits,
                CharacterDataAccess.CharacterHitsBuffer,
                CharacterDataAccess.DeferredImpulsesBuffer,
                ref characterPosition);
        }

        void HandleVelocityControl(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            float deltaTime = baseContext.Time.DeltaTime;
            ref KinematicCharacterBody characterBody = ref CharacterDataAccess.CharacterBody.ValueRW;
            ref CharacterData characterData = ref CharacterData.ValueRW;
            ref CharacterInputData inputData = ref CharacterInputData.ValueRW;
            ref CharacterState characterState = ref CharacterState.ValueRW;

            // Update state
            characterState.IsGrounded = characterBody.IsGrounded;
            characterState.IsSprinting = inputData.SprintPressed;
            characterState.IsCrouching = inputData.CrouchPressed;

            // Calculate speed based on state
            float speed = characterState.IsSprinting ? characterData.RunSpeed : characterData.WalkSpeed;
            if (characterState.IsCrouching) speed = characterData.CrouchSpeed;

            // Create move vector from input
            float3 moveVector = new float3(inputData.MoveInput.x, 0f, inputData.MoveInput.y);

            if (characterBody.IsGrounded)
            {
                // Grounded movement
                float3 targetVelocity = moveVector * speed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(
                    ref characterBody.RelativeVelocity,
                    targetVelocity,
                    15f, // Sharpness
                    deltaTime,
                    characterBody.GroundingUp,
                    characterBody.GroundHit.Normal);

                // Jump
                if (inputData.JumpPressed)
                {
                    CharacterControlUtilities.StandardJump(
                        ref characterBody,
                        characterBody.GroundingUp * characterData.JumpHeight,
                        true,
                        characterBody.GroundingUp);
                    characterState.IsJumping = true;
                }
                else
                {
                    characterState.IsJumping = false;
                }
            }
            else
            {
                // Air movement
                float3 airAcceleration = moveVector * 50f; // Air acceleration
                if (math.lengthsq(airAcceleration) > 0f)
                {
                    CharacterControlUtilities.StandardAirMove(
                        ref characterBody.RelativeVelocity,
                        airAcceleration,
                        speed,
                        characterBody.GroundingUp,
                        deltaTime,
                        false);
                }

                // Gravity
                CharacterControlUtilities.AccelerateVelocity(
                    ref characterBody.RelativeVelocity,
                    math.down() * 20f, // Gravity
                    deltaTime);

                characterState.IsJumping = false;
            }
        }

        public void VariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            ref quaternion characterRotation = ref CharacterDataAccess.LocalTransform.ValueRW.Rotation;
            ref CharacterInputData inputData = ref CharacterInputData.ValueRW;

            // Handle rotation
            float3 moveVector = new float3(inputData.MoveInput.x, 0f, inputData.MoveInput.y);
            if (math.lengthsq(moveVector) > 0.001f)
            {
                quaternion targetRotation = quaternion.LookRotation(moveVector, math.up());
                characterRotation = math.slerp(characterRotation, targetRotation, baseContext.Time.DeltaTime * 10f);
            }
        }

        #region Character Processor Callbacks
        public void UpdateGroundingUp(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext)
        {
            ref KinematicCharacterBody characterBody = ref CharacterDataAccess.CharacterBody.ValueRW;
            KinematicCharacterUtilities.Default_UpdateGroundingUp(
                ref characterBody,
                CharacterDataAccess.LocalTransform.ValueRO.Rotation);
        }

        public bool CanCollideWithHit(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit)
        {
            return PhysicsUtilities.IsCollidable(hit.Material);
        }

        public bool IsGroundedOnHit(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit,
            int groundingEvaluationType)
        {
            return hit.Normal.y > 0.7f; // Simple slope check
        }

        public void OnMovementHit(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref KinematicCharacterHit hit,
            ref float3 remainingMovementDirection,
            ref float remainingMovementLength,
            float3 originalVelocityDirection,
            float hitDistance)
        {
            // Default behavior
        }

        public void OverrideDynamicHitMasses(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref Unity.Physics.PhysicsMass characterMass,
            ref Unity.Physics.PhysicsMass otherMass,
            BasicHit hit)
        {
        }

        public void ProjectVelocityOnHits(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref float3 velocity,
            ref bool characterIsGrounded,
            ref BasicHit characterGroundHit,
            in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
            float3 originalVelocityDirection)
        {
            KinematicCharacterUtilities.Default_ProjectVelocityOnHits(
                ref velocity,
                ref characterIsGrounded,
                ref characterGroundHit,
                in velocityProjectionHits,
                originalVelocityDirection,
                false,
                in CharacterDataAccess.CharacterBody.ValueRO);
        }
        #endregion
    }
}
