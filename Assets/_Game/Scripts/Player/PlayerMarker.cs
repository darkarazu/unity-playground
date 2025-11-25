using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Game.Components;

namespace Game.Player
{
    /// <summary>
    /// Creates a minimal ECS entity that tracks the MonoBehaviour player's position.
    /// This allows NPCs to detect the player using the PlayerTag ECS component.
    /// </summary>
    public class PlayerMarker : MonoBehaviour
    {
        private Entity markerEntity;
        private EntityManager entityManager;
        private bool entityCreated = false;

        private void Start()
        {
            CreateMarkerEntity();
        }

        private void CreateMarkerEntity()
        {
            // Get the default world and entity manager
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("PlayerMarker: No default world found. Make sure ECS is initialized.");
                return;
            }

            entityManager = world.EntityManager;

            // Create a minimal entity with Transform and PlayerTag
            markerEntity = entityManager.CreateEntity(
                typeof(LocalTransform),
                typeof(PlayerTag)
            );

            // Set initial position
            UpdateMarkerPosition();
            
            entityCreated = true;
            
            Debug.Log($"PlayerMarker: Created ECS entity {markerEntity} with PlayerTag");
        }

        private void Update()
        {
            if (entityCreated)
            {
                UpdateMarkerPosition();
            }
        }

        private void UpdateMarkerPosition()
        {
            // Check if EntityManager is still valid
            if (entityManager == null)
                return;
                
            // Update the marker entity's position to match the player GameObject
            if (entityManager.Exists(markerEntity))
            {
                var transform = new LocalTransform
                {
                    Position = new float3(this.transform.position.x, 
                                         this.transform.position.y, 
                                         this.transform.position.z),
                    Rotation = quaternion.identity,
                    Scale = 1f
                };

                entityManager.SetComponentData(markerEntity, transform);
            }
        }

        private void OnDestroy()
        {
            // Clean up the marker entity when player is destroyed
            // Check if world still exists (it gets destroyed first when exiting play mode)
            var world = World.DefaultGameObjectInjectionWorld;
            if (entityCreated && world != null && world.IsCreated)
            {
                // World and EntityManager are always created/destroyed together
                // So if world.IsCreated is true, EntityManager is valid
                if (world.EntityManager.Exists(markerEntity))
                {
                    world.EntityManager.DestroyEntity(markerEntity);
                    Debug.Log("PlayerMarker: Destroyed ECS entity");
                }
            }
        }
    }
}
