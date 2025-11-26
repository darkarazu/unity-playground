using Unity.Entities;

namespace Game.Components
{
    /// <summary>
    /// Data component for NPC target tracking (e.g., following player).
    /// </summary>
    public struct NPCTargetData : IComponentData
    {
        /// <summary>
        /// Entity to follow (typically the player).
        /// </summary>
        public Entity TargetEntity;

        /// <summary>
        /// Distance at which the NPC starts following the target.
        /// </summary>
        public float FollowDetectionRange;

        /// <summary>
        /// Minimum distance to maintain from target when following.
        /// </summary>
        public float FollowDistance;

        /// <summary>
        /// Maximum distance from patrol center before returning.
        /// If the NPC is following and exceeds this distance from patrol center, it returns.
        /// </summary>
        public float MaxDistanceFromPatrolCenter;

        /// <summary>
        /// Maximum time (in seconds) the NPC can spend following a target before giving up.
        /// Prevents NPCs from getting stuck trying to follow unreachable targets.
        /// </summary>
        public float MaxFollowTime;
    }
}
