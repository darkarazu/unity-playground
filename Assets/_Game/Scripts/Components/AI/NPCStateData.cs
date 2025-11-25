using Unity.Entities;

namespace Game.Components
{
    /// <summary>
    /// Enum defining the possible states for NPC AI.
    /// </summary>
    public enum NPCState
    {
        /// <summary>
        /// NPC is patrolling within its patrol area.
        /// </summary>
        Patrolling,

        /// <summary>
        /// NPC is following a target (e.g., player).
        /// </summary>
        Following,

        /// <summary>
        /// NPC is returning to its patrol area.
        /// </summary>
        Returning
    }

    /// <summary>
    /// Data component tracking the current state of the NPC AI.
    /// </summary>
    public struct NPCStateData : IComponentData
    {
        /// <summary>
        /// Current AI state.
        /// </summary>
        public NPCState CurrentState;

        /// <summary>
        /// Time spent in current state (for debugging/state duration tracking).
        /// </summary>
        public float TimeInCurrentState;
    }
}
