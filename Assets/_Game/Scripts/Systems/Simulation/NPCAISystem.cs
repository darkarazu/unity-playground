using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Game.Components;
using System.Diagnostics;

namespace Game.Systems
{
    /// <summary>
    /// System that generates AI input for NPCs based on their current state.
    /// Writes to CharacterInputData which is then processed by the movement systems.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct NPCAISystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NPCTag>();
        }

        [BurstCompile(Debug = true)]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Process each NPC
            foreach (var (inputData, patrolData, targetData, stateData, transform, entity) in
                     SystemAPI.Query<RefRW<CharacterInputData>,
                                    RefRW<NPCPatrolData>,
                                    RefRO<NPCTargetData>,
                                    RefRW<NPCStateData>,
                                    RefRO<LocalTransform>>()
                             .WithAll<NPCTag>()
                             .WithEntityAccess())
            {
                ref var input = ref inputData.ValueRW;
                ref var patrol = ref patrolData.ValueRW;
                var target = targetData.ValueRO;  // No ref - this is readonly
                ref var aiState = ref stateData.ValueRW;
                float3 npcPosition = transform.ValueRO.Position;

                // Update time in state
                aiState.TimeInCurrentState += deltaTime;

                // Find the player entity dynamically using PlayerTag
                // This is more robust than using a baked reference, especially with subscenes
                float3 targetPosition = float3.zero;
                float distanceToTarget = float.MaxValue;
                bool hasValidTarget = false;

                // Look for player entity with PlayerTag
                foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
                {
                    targetPosition = playerTransform.ValueRO.Position;
                    distanceToTarget = math.distance(npcPosition, targetPosition);
                    hasValidTarget = true;
                    break; // Only need the first player
                }

                // Calculate distance from patrol center
                float distanceFromPatrolCenter = math.distance(npcPosition, patrol.PatrolCenter);
                // State machine logic
                switch (aiState.CurrentState)
                {
                    case NPCState.Patrolling:
                        HandlePatrolling(ref state, ref input, ref patrol, ref aiState, npcPosition, deltaTime,
                                       hasValidTarget, distanceToTarget, in target, distanceFromPatrolCenter);
                        break;

                    case NPCState.Following:
                        HandleFollowing(ref input, ref aiState, npcPosition, targetPosition,
                                      hasValidTarget, distanceToTarget, in target, distanceFromPatrolCenter);
                        break;

                    case NPCState.Returning:
                        HandleReturning(ref state, ref input, ref patrol, ref aiState, npcPosition);
                        break;
                }
            }
        }

        [BurstCompile]
        private void HandlePatrolling(ref SystemState systemState, ref CharacterInputData input, ref NPCPatrolData patrol,
                                     ref NPCStateData state, float3 npcPosition, float deltaTime,
                                     bool hasValidTarget, float distanceToTarget, in NPCTargetData target,
                                     float distanceFromPatrolCenter)
        {
            // Check if should start following target
            if (hasValidTarget && distanceToTarget <= target.FollowDetectionRange)
            {
                // Store current position to return to later
                state.ReturnPosition = npcPosition;
                TransitionToState(ref state, NPCState.Following);
                return;
            }

            // Check if at patrol point
            float distanceToPatrolTarget = math.distance(npcPosition, patrol.CurrentPatrolTarget);

            if (distanceToPatrolTarget <= patrol.PatrolTargetReachedDistance)
            {
                // Reached patrol point - wait here
                patrol.CurrentWaitTime += deltaTime;
                patrol.TimeSpentOnCurrentPatrolTarget = 0f; // Reset stuck timer

                if (patrol.CurrentWaitTime >= patrol.WaitTimeAtPatrolPoint)
                {
                    // Select new patrol target
                    patrol.CurrentPatrolTarget = GenerateRandomPatrolPoint(ref systemState, patrol.PatrolCenter, patrol.PatrolRadius);
                    
                    // Randomize wait time for next patrol point
                    uint seed = (uint)(systemState.WorldUnmanaged.Time.ElapsedTime * 1000000);
                    Random random = new Random(seed);
                    patrol.WaitTimeAtPatrolPoint = random.NextFloat(patrol.MinWaitTimeAtPatrolPoint, patrol.MaxWaitTimeAtPatrolPoint);
                    patrol.CurrentWaitTime = 0f;
                    patrol.TimeSpentOnCurrentPatrolTarget = 0f;
                }

                // Stop moving while waiting
                input.MoveInput = float2.zero;
            }
            else
            {
                // Moving toward patrol target - track time spent
                patrol.TimeSpentOnCurrentPatrolTarget += deltaTime;
                
                // TIMEOUT CHECK: If stuck trying to reach point for too long, get new point
                const float maxPatrolTargetTimeout = 15f; // 15 seconds timeout
                if (patrol.TimeSpentOnCurrentPatrolTarget > maxPatrolTargetTimeout)
                {
                    // NPC is stuck - generate new patrol point
                    patrol.CurrentPatrolTarget = GenerateRandomPatrolPoint(ref systemState, patrol.PatrolCenter, patrol.PatrolRadius);
                    
                    // Randomize wait time for next patrol point
                    uint seed = (uint)(systemState.WorldUnmanaged.Time.ElapsedTime * 1000000);
                    Random random = new Random(seed);
                    patrol.WaitTimeAtPatrolPoint = random.NextFloat(patrol.MinWaitTimeAtPatrolPoint, patrol.MaxWaitTimeAtPatrolPoint);
                    
                    patrol.TimeSpentOnCurrentPatrolTarget = 0f;
                    patrol.CurrentWaitTime = 0f;
                }
                
                // Move toward patrol target
                float3 direction = math.normalize(patrol.CurrentPatrolTarget - npcPosition);
                input.MoveInput = new float2(direction.x, direction.z);
            }

            // Reset other inputs
            input.JumpPressed = false;
            input.SprintPressed = false;
            input.CrouchPressed = false;
        }

        [BurstCompile]
        private void HandleFollowing(ref CharacterInputData input, ref NPCStateData state,
                                    float3 npcPosition, float3 targetPosition,
                                    bool hasValidTarget, float distanceToTarget, in NPCTargetData target,
                                    float distanceFromPatrolCenter)
        {
            // TIMEOUT CHECK: If following for too long (stuck trying to reach unreachable target), give up
            if (state.TimeInCurrentState > target.MaxFollowTime)
            {
                TransitionToState(ref state, NPCState.Returning);
                return;
            }
            
            // Check if should return to patrol (too far from patrol center or target lost)
            if (!hasValidTarget || 
                distanceFromPatrolCenter > target.MaxDistanceFromPatrolCenter ||
                distanceToTarget > target.FollowDetectionRange * 1.5f) // Add hysteresis
            {
                TransitionToState(ref state, NPCState.Returning);
                return;
            }

            // Move toward target, but maintain follow distance
            if (distanceToTarget > target.FollowDistance)
            {
                float3 direction = math.normalize(targetPosition - npcPosition);
                input.MoveInput = new float2(direction.x, direction.z);
                input.SprintPressed = true; // Sprint when following
            }
            else
            {
                // Close enough, stop moving
                input.MoveInput = float2.zero;
                input.SprintPressed = false;
            }

            // Reset other inputs
            input.JumpPressed = false;
            input.CrouchPressed = false;
        }

        [BurstCompile]
        private void HandleReturning(ref SystemState systemState, ref CharacterInputData input, ref NPCPatrolData patrol,
                                    ref NPCStateData state, float3 npcPosition)
        {
            float distanceToReturnPoint = math.distance(npcPosition, state.ReturnPosition);
            
            // Check if close to return position (where NPC was when it started following)
            const float arrivedThreshold = 2f; // Consider arrived when within 2 units
            if (distanceToReturnPoint <= arrivedThreshold)
            {
                // Reached return position - generate new patrol target and switch to patrolling
                patrol.CurrentPatrolTarget = GenerateRandomPatrolPoint(ref systemState, patrol.PatrolCenter, patrol.PatrolRadius);
                
                // Randomize wait time for next patrol point
                uint seed = (uint)(systemState.WorldUnmanaged.Time.ElapsedTime * 1000000);
                Random random = new Random(seed);
                patrol.WaitTimeAtPatrolPoint = random.NextFloat(patrol.MinWaitTimeAtPatrolPoint, patrol.MaxWaitTimeAtPatrolPoint);
                
                patrol.CurrentWaitTime = 0f;
                patrol.TimeSpentOnCurrentPatrolTarget = 0f;
                TransitionToState(ref state, NPCState.Patrolling);
                return;
            }

            // Move toward return position
            float3 direction = math.normalize(state.ReturnPosition - npcPosition);
            input.MoveInput = new float2(direction.x, direction.z);
            input.SprintPressed = true; // Sprint when returning

            // Reset other inputs
            input.JumpPressed = false;
            input.CrouchPressed = false;
        }

        [BurstCompile]
        private void TransitionToState(ref NPCStateData state, NPCState newState)
        {
            state.CurrentState = newState;
            state.TimeInCurrentState = 0f;
        }

        [BurstCompile]
        private float3 GenerateRandomPatrolPoint(ref SystemState state, float3 center, float radius)
        {
            // Generate random point within patrol radius
            uint seed = (uint)(state.WorldUnmanaged.Time.ElapsedTime * 1000000);
            Random random = new Random(seed);

            float angle = random.NextFloat(0f, math.PI * 2f);
            float distance = random.NextFloat(0f, radius);

            float x = center.x + math.cos(angle) * distance;
            float z = center.z + math.sin(angle) * distance;

            // Use raycast to find terrain height at this X/Z position
            // Start high above to ensure we're above any terrain
            float3 rayStart = new float3(x, center.y + 100f, z);
            float3 rayEnd = new float3(x, center.y - 100f, z); // Cast far down
            
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            
            // Create collision filter that collides with everything (including terrain)
            var raycastInput = new RaycastInput
            {
                Start = rayStart,
                End = rayEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,      // Belongs to all layers
                    CollidesWith = ~0u,   // Collides with all layers
                    GroupIndex = 0
                }
            };
            
            float yHeight = center.y; // Default to center height if no hit
            
            if (physicsWorld.CastRay(raycastInput, out RaycastHit hit))
            {
                // Use the hit point's Y coordinate (terrain surface) + small offset
                yHeight = hit.Position.y + 0.1f; // Small offset to ensure NPC is above ground
            }

            return new float3(x, yHeight, z);
        }
    }
}
