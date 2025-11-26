using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;

namespace Game.Scripts.Hybrid
{
    /// <summary>
    /// Generates an ECS PhysicsCollider for a Unity Terrain at runtime.
    /// This allows the Terrain to remain in the Main Scene (visible, works with Player)
    /// while still providing collision for ECS NPCs.
    /// </summary>
    public class TerrainPhysicsRuntime : MonoBehaviour
    {
        [Tooltip("The Terrain to generate physics for. If null, uses GetComponent<Terrain>().")]
        public Terrain Terrain;

        private Entity _terrainEntity;
        private BlobAssetReference<Unity.Physics.Collider> _colliderBlob;

        void Start()
        {
            if (Terrain == null) Terrain = GetComponent<Terrain>();
            if (Terrain == null)
            {
                Debug.LogError("TerrainPhysicsRuntime: No Terrain found!");
                return;
            }

            CreateTerrainEntity();
        }

        void CreateTerrainEntity()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;
            var terrainData = Terrain.terrainData;
            
            // 1. Get Terrain Data
            var resolution = terrainData.heightmapResolution;
            var scale = terrainData.heightmapScale;
            var size = new int2(resolution, resolution);

            // 2. Flatten Heights
            // Note: This can be expensive for large terrains. 
            // For production, consider using a pre-baked asset or jobifying this.
            var heights = terrainData.GetHeights(0, 0, resolution, resolution);
            var flatHeights = new NativeArray<float>(resolution * resolution, Allocator.Temp);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    flatHeights[x + y * resolution] = heights[y, x];
                }
            }

            // 3. Create Physics Collider
            _colliderBlob = Unity.Physics.TerrainCollider.Create(
                flatHeights,
                size,
                scale,
                Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples
            );

            flatHeights.Dispose();

            // 4. Create Entity
            _terrainEntity = entityManager.CreateEntity();
            
            // 5. Add Components
            // PhysicsCollider
            entityManager.AddComponentData(_terrainEntity, new PhysicsCollider { Value = _colliderBlob });
            
            // PhysicsWorldIndex (Default) - use AddSharedComponentManaged to add it to the entity
            entityManager.AddSharedComponentManaged(_terrainEntity, new PhysicsWorldIndex());

            // LocalTransform (Position/Rotation from GameObject)
            // Note: Terrain pivot is usually (0,0,0) relative to the terrain mesh, 
            // but we should respect the GameObject's transform.
            entityManager.AddComponentData(_terrainEntity, LocalTransform.FromPositionRotation(
                transform.position, 
                transform.rotation
            ));

            // LocalToWorld - CRITICAL for ECS Physics to know where the entity is in world space!
            // This is updated automatically by the transform system
            entityManager.AddComponentData(_terrainEntity, new LocalToWorld
            {
                Value = float4x4.TRS(transform.position, transform.rotation, new float3(1, 1, 1))
            });

            Debug.Log($"<color=green>TerrainPhysicsRuntime: Created terrain entity with collider. " +
                      $"Resolution: {resolution}, Scale: {scale}, Position: {transform.position}</color>");
        }

        void OnDestroy()
        {
            // Clean up the entity and dispose the blob asset to prevent memory leaks
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                var entityManager = world.EntityManager;
                if (entityManager.Exists(_terrainEntity))
                {
                    entityManager.DestroyEntity(_terrainEntity);
                }
            }

            // Dispose the collider blob asset
            if (_colliderBlob.IsCreated)
            {
                _colliderBlob.Dispose();
            }
        }
    }
}
