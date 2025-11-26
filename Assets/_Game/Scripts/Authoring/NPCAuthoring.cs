using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Game.Components;

namespace Game.Authoring
{
    /// <summary>
    /// Authoring component for creating NPC entities with AI behavior.
    /// Add this to a GameObject with CharacterAuthoring to make it an NPC.
    /// </summary>
    public class NPCAuthoring : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [Tooltip("Center of the patrol area (local to this GameObject's position)")]
        public Vector3 patrolCenterOffset = Vector3.zero;

        [Tooltip("Radius of the patrol area")]
        [Min(1f)]
        public float patrolRadius = 10f;

        [Tooltip("Distance to patrol target before considering it reached")]
        [Min(0.1f)]
        public float patrolTargetReachedDistance = 1f;

        [Tooltip("Minimum time to wait at each patrol point (in seconds)")]
        [Min(0f)]
        public float minWaitTimeAtPatrolPoint = 1f;

        [Tooltip("Maximum time to wait at each patrol point (in seconds)")]
        [Min(0f)]
        public float maxWaitTimeAtPatrolPoint = 3f;

        [Header("Follow Settings")]
        [Tooltip("Target to follow (typically the player GameObject)")]
        public GameObject targetToFollow;

        [Tooltip("Distance at which NPC detects and starts following the target")]
        [Min(1f)]
        public float followDetectionRange = 15f;

        [Tooltip("Minimum distance to maintain from target when following")]
        [Min(0.5f)]
        public float followDistance = 3f;

        [Tooltip("Maximum distance from patrol center before NPC returns")]
        [Min(1f)]
        public float maxDistanceFromPatrolCenter = 25f;

        [Tooltip("Maximum time (in seconds) NPC can follow target before giving up")]
        [Min(1f)]
        public float maxFollowTime = 20f;

        [Header("Visualization")]
        [Tooltip("Show debug gizmos in scene view")]
        public bool showDebugGizmos = true;

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            Vector3 patrolCenter = transform.position + patrolCenterOffset;

            // Draw max distance from patrol center (RED - draw first so it's behind)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            DrawWireCircle(patrolCenter, maxDistanceFromPatrolCenter, 32);

            // Draw patrol area (GREEN)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            DrawWireCircle(patrolCenter, patrolRadius, 32);

            // Draw patrol center
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolCenter, 0.5f);

            // Draw follow detection range (YELLOW)
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            DrawWireCircle(transform.position, followDetectionRange, 32);

            // Draw connection to target (CYAN)
            if (targetToFollow != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, targetToFollow.transform.position);
                
                // Draw a sphere at target position
                Gizmos.DrawWireSphere(targetToFollow.transform.position, 0.5f);
            }
        }

        private void DrawWireCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        class Baker : Baker<NPCAuthoring>
        {
            public override void Bake(NPCAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add NPC tag
                AddComponent<NPCTag>(entity);

                // Add patrol data
                float3 offsetAsFloat3 = new float3(authoring.patrolCenterOffset.x, authoring.patrolCenterOffset.y, authoring.patrolCenterOffset.z);
                float3 positionAsFloat3 = new float3(authoring.transform.position.x, authoring.transform.position.y, authoring.transform.position.z);
                float3 patrolCenter = positionAsFloat3 + offsetAsFloat3;
                
                // Generate initial random patrol target to avoid synchronized movement
                uint seed = (uint)(UnityEngine.Random.value * uint.MaxValue);
                Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed == 0 ? 1 : seed);
                float angle = random.NextFloat(0f, math.PI * 2f);
                float distance = random.NextFloat(0f, authoring.patrolRadius);
                
                // Calculate X/Z position
                float targetX = patrolCenter.x + math.cos(angle) * distance;
                float targetZ = patrolCenter.z + math.sin(angle) * distance;
                
                // Sample terrain height at this X/Z position (edit-time)
                float targetY = patrolCenter.y; // Default height
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    Vector3 worldPos = new Vector3(targetX, 0, targetZ);
                    targetY = terrain.SampleHeight(worldPos);
                }
                
                float3 initialPatrolTarget = new float3(targetX, targetY, targetZ);
                
                // Randomize initial wait time
                float initialWaitTime = random.NextFloat(authoring.minWaitTimeAtPatrolPoint, authoring.maxWaitTimeAtPatrolPoint);
                
                AddComponent(entity, new NPCPatrolData
                {
                    PatrolCenter = patrolCenter,
                    PatrolRadius = authoring.patrolRadius,
                    CurrentPatrolTarget = initialPatrolTarget, // Randomized starting position
                    PatrolTargetReachedDistance = authoring.patrolTargetReachedDistance,
                    MinWaitTimeAtPatrolPoint = authoring.minWaitTimeAtPatrolPoint,
                    MaxWaitTimeAtPatrolPoint = authoring.maxWaitTimeAtPatrolPoint,
                    WaitTimeAtPatrolPoint = initialWaitTime, // Randomized initial wait time
                    CurrentWaitTime = 0f,
                    TimeSpentOnCurrentPatrolTarget = 0f
                });

                // Add target data
                Entity targetEntity = Entity.Null;
                if (authoring.targetToFollow != null)
                {
                    targetEntity = GetEntity(authoring.targetToFollow, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new NPCTargetData
                {
                    TargetEntity = targetEntity,
                    FollowDetectionRange = authoring.followDetectionRange,
                    FollowDistance = authoring.followDistance,
                    MaxDistanceFromPatrolCenter = authoring.maxDistanceFromPatrolCenter,
                    MaxFollowTime = authoring.maxFollowTime
                });

                // Add state data (start in patrolling state)
                AddComponent(entity, new NPCStateData
                {
                    CurrentState = NPCState.Patrolling,
                    TimeInCurrentState = 0f,
                    ReturnPosition = patrolCenter // Initialize to patrol center
                });

                // Note: CharacterInputData is already added by CharacterAuthoring
                // The AI system will write to it
            }
        }
    }
}
