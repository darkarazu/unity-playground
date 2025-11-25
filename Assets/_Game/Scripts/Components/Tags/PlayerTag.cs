using Unity.Entities;

namespace Game.Components
{
    /// <summary>
    /// Tag component to identify player-controlled entities.
    /// Used to filter player input processing.
    /// </summary>
    public struct PlayerTag : IComponentData
    {
    }
}
