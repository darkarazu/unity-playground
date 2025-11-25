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
        /// Time to wait at each patrol point before moving to next (in seconds).
        /// </summary>
        public float WaitTimeAtPatrolPoint;

        /// <summary>
        /// Current wait timer.
        /// </summary>
        public float CurrentWaitTime;
    }
}
