using Unity.Entities;
using Unity.Mathematics;

namespace Game.Components
{
    /// <summary>
    /// Data component for NPC patrol behavior.
    /// Defines the patrol area and current patrol target.
    /// </summary>
    public struct NPCPatrolData : IComponentData
    {
        /// <summary>
        /// Center position of the patrol area (world space).
        /// </summary>
        public float3 PatrolCenter;

        /// <summary>
        /// Radius of the patrol area.
        /// </summary>
        public float PatrolRadius;

        /// <summary>
        /// Current target position within the patrol area.
        /// </summary>
        public float3 CurrentPatrolTarget;

        /// <summary>
        /// Minimum distance to patrol target before selecting a new one.
        /// </summary>
        public float PatrolTargetReachedDistance;

        /// <summary>
        /// Minimum time to wait at each patrol point (in seconds).
        /// </summary>
        public float MinWaitTimeAtPatrolPoint;

        /// <summary>
        /// Maximum time to wait at each patrol point (in seconds).
        /// </summary>
        public float MaxWaitTimeAtPatrolPoint;

        /// <summary>
        /// Current wait time target (randomized between min and max).
        /// </summary>
        public float WaitTimeAtPatrolPoint;

        /// <summary>
        /// Current wait timer.
        /// </summary>
        public float CurrentWaitTime;

        /// <summary>
        /// Time spent trying to reach the current patrol target (in seconds).
        /// Used to detect stuck NPCs and generate new target if timeout exceeded.
        /// </summary>
        public float TimeSpentOnCurrentPatrolTarget;
    }
}
