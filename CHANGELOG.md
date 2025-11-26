# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2025-11-26

### Terrain Support for Hybrid ECS Architecture

#### Added
- **Terrain System**: Full terrain support with hybrid physics (Player + ECS NPCs)
  - **TerrainPhysicsRuntime.cs**: Runtime ECS physics collider generation
    - Samples heightmap data at runtime
    - Converts to ECS `PhysicsCollider` using `TerrainCollider.Create()`
    - Adds `LocalToWorld` component for proper world-space positioning
    - Includes proper cleanup (`OnDestroy`) to prevent memory leaks
  - **Hybrid Physics Support**: 
    - Player collision via standard `TerrainCollider` (CharacterController)
    - NPC collision via ECS `PhysicsCollider` (Unity.Physics)
  - Both systems work simultaneously without interference
- **Documentation**: `Terrain.md` comprehensive setup guide
  - Step-by-step terrain creation and physics configuration
  - Troubleshooting section for common issues
  - Clear instructions for Main Scene placement (NOT subscene)
- **Project Planning**: `ROADMAP.md` development roadmap
  - 22 open world RPG mechanics researched and documented
  - Ordered by implementation difficulty (Easy → Extremely Hard)
  - ECS vs MonoBehaviour guidance for each system
  - Short/Medium/Long term development timeline
  - References from AAA titles (Skyrim, Witcher 3, Zelda, Elden Ring)

#### Changed
- **Terrain Workflow**: Terrain must be in Main Scene (not subscene)
  - Ensures terrain remains visible in Play Mode
  - Allows standard rendering while ECS physics entity runs separately
- **NPC Positioning**: NPCs must be placed at terrain surface height
  - Patrol centers initialize at spawn position
  - Prevents floating waypoints when NPCs fall to terrain surface

#### Fixed
- **Unity Physics API Compatibility**: Fixed for Unity 6
  - Removed `TerrainGeometry` struct (not exposed in API)
  - Used direct parameter overload: `TerrainCollider.Create(heights, size, scale, method)`
  - Fixed `CollisionMethod` enum (changed `Triangulate` → `VertexSamples`)
- **Shared Component Error**: Changed `SetSharedComponentManaged` to `AddSharedComponentManaged`
  - `PhysicsWorldIndex` is a shared component, needs Add not Set
- **ECS Physics Entity Positioning**: Added `LocalToWorld` component
  - Critical for Unity.Physics to locate entities in world space
  - Static bodies require both `LocalTransform` and `LocalToWorld`
- **Memory Leak**: Proper blob asset disposal in `OnDestroy()`
  - Store `BlobAssetReference<Collider>` and dispose when MonoBehaviour destroyed
  - Destroy terrain entity when component removed

#### Technical Notes
- **Runtime Approach**: Terrain generates ECS collider at runtime (not baked)
  - Allows terrain to remain as visible GameObject in Main Scene
  - `TerrainPhysicsRuntime` runs in `Start()`, creates entity in ECS world
- **Collision Methods**: Using `VertexSamples` for performance
  - Fast approximation suitable for smooth terrain
  - Can be changed to other methods if needed
- **Debug Logging**: Green console message confirms successful terrain entity creation
  - Displays resolution, scale, and position for verification

---

## [0.4.0] - 2025-11-25

### Third Person Camera System - WoW-Style Controls

#### Added
- **Cinemachine Integration**: Professional 3rd person camera using Cinemachine 3.x
  - FreeLook camera with three orbital rigs (Top/Center/Bottom)
  - Smooth camera follow and orbit behavior
  - Damping for smooth transitions
- **CameraController**: WoW-style camera control system
  - **Left Mouse Button (LMB)**: Orbit camera around player (view only)
  - **Right Mouse Button (RMB)**: Smoothly rotate player to face camera direction
  - **Scroll Wheel**: Zoom in/out with proportional scaling (radius + height)
  - **Dynamic FOV**: Smooth transitions between normal and sprint FOV (only when moving)
  - Mouse sensitivity settings (configurable)
  - Zoom sensitivity and min/max distance settings
  - Player rotation speed for smooth turning (Quaternion.Slerp)
  - FOV settings (normal, sprint, transition speed)
  - Invert Y-axis option
  - Direct control of Cinemachine Orbital Follow axes
  - Proportional zoom maintains orbital sphere shape
  - FOV only widens when sprinting AND actually moving
- **Camera Documentation**: `Camera.md` comprehensive guide
  - Complete setup instructions for Cinemachine 3
  - Troubleshooting section
  - Customization options (distance, height, sensitivity, zoom, FOV, rotation)
  - Architecture notes
- **CinemachineBrain Setup**: Instructions for Main Camera configuration

#### Changed
- **PlayerController Movement**: Migrated to WoW-style strafe controls
  - **W**: Move forward (relative to player facing)
  - **S**: Move backward (relative to player facing)
  - **A**: Strafe left (relative to player facing)
  - **D**: Strafe right (relative to player facing)
  - Removed automatic rotation toward movement direction
  - Added `SetYRotation()` method for camera-controlled rotation
- **Movement System**: Camera-relative instead of world-space
  - Player rotation controlled by camera (RMB)
  - Movement relative to player's current facing direction

#### Dependencies
- **Cinemachine 3.x**: Added as required package for camera system

#### Technical Notes
- Clean separation: CameraController handles camera/rotation, PlayerController handles movement
- Compatible with existing NPC AI system (NPCs unaffected)
- Uses Cinemachine 3 API (`CinemachineCamera`, `CinemachineOrbitalFollow`, `CinemachineRotationComposer`)
- Input handled via Unity Input System (programmatic actions)

---

## [0.3.0] - 2025-11-25

### Hybrid Architecture - MonoBehaviour Player

#### Changed
- **Player Architecture**: Migrated player from ECS to MonoBehaviour
  - Created `PlayerController` MonoBehaviour for input and movement
  - Uses Unity's built-in `CharacterController` for physics
  - Simpler code, easier integration with Unity features
  - Traditional workflow for single-character control
- **PlayerMarker System**: Bridge between MonoBehaviour player and ECS NPCs
  - Creates minimal ECS entity with `PlayerTag` and `LocalTransform`
  - Automatically syncs position with player GameObject
  - Allows NPCs to detect player using ECS queries
- **CharacterAuthoring**: Now used exclusively for NPCs
  - Removed `IsPlayerControlled` field
  - Simplified to NPC-only use case
  - PlayerTag baking removed (handled by PlayerMarker)
- **Hybrid Architecture**: Best of both worlds
  - MonoBehaviour for player (simplicity, Unity integration)
  - ECS for NPCs (performance, scalability)

#### Added
- **PlayerController**: Full-featured MonoBehaviour player controller
  - Input handling using Unity Input System
  - Movement: walk, run, crouch speeds
  - Jumping with configurable height
  - Gravity with ground detection
  - Same controls as previous ECS version
- **PlayerMarker**: Automatic ECS entity creation and sync
  - Creates entity with PlayerTag on Start
  - Updates position every frame
  - Cleans up entity on Destroy (with Unity 6 API compatibility)
- **Documentation**: `Playable_Character.md` comprehensive setup guide
- **Ground Setup**: Detailed instructions for hybrid physics setup
  - Ground must be in subscene with Physics Shape for ECS
  - Standard colliders work for MonoBehaviour player

#### Removed
- **CharacterInputSystem**: No longer needed for player (NPCs still use their own AI input)
- **ECS Player Complexity**: Eliminated ECS overhead for single player character

#### Fixed
- **EntityManager disposal errors**: PlayerMarker now checks `World.IsCreated` before cleanup
- **Ground physics**: Clarified subscene requirement for Physics Shape baking

#### Technical Notes
- NPCs unchanged - still use ECS and detect player via PlayerTag
- Clean separation: GameObject player, Entity NPCs
- Minimal performance overhead (one entity update per frame)
- Easy to extend player with animations, UI, audio

---

## [0.2.0] - 2025-11-25

### NPC AI System Implementation

#### Added
- **NPC AI System**: Complete AI behavior system for non-player characters
  - **Tag Components**: `PlayerTag` and `NPCTag` for entity identification
  - **AI Components**: 
    - `NPCPatrolData` for patrol area configuration
    - `NPCTargetData` for target tracking and follow behavior
    - `NPCStateData` for AI state machine (Patrolling/Following/Returning)
  - **NPCAISystem**: ECS system generating AI input for NPCs
    - Patrol behavior: Random waypoints within configurable radius
    - Follow behavior: Detects and follows player using PlayerTag
    - Return behavior: Returns to patrol area when too far from center
    - Dynamic player detection using PlayerTag query (works across subscenes)
  - **NPCAuthoring**: Unity Inspector component for NPC configuration
    - Visual debug gizmos for patrol area, detection range, and boundaries
    - All parameters configurable in Inspector
- **Documentation**: `NPC_AI.md` setup guide with examples and troubleshooting

#### Changed
- **CharacterAuthoring**: Added `IsPlayerControlled` field to distinguish players from NPCs
- **CharacterInputSystem**: Updated to filter player entities using `PlayerTag`
- **Architecture**: Demonstrates input reusability - both players and NPCs use `CharacterInputData`, populated by different systems

#### Fixed
- Unity 6 API compatibility issues with `SystemAPI.Time` and readonly references
- Dynamic player entity detection resolving subscene entity reference issues
- System update ordering warnings
- Gizmo visibility improvements with better color opacity

#### Technical Notes
- Full Burst compilation and job scheduling support
- Scales efficiently with many NPCs
- Works in both hierarchy and subscene workflows
- Compatible with Unity 6 ECS

---

## [0.1.0] - 2025-11-24

### Initial Release - ECS Character Controller Prototype

#### Added
- **Project Structure**: Established `Assets/_Game/` directory structure separating user content from third-party assets
- **ECS Architecture**: Implemented core ECS folder structure (`Components`, `Systems`, `Authoring`)
- **Character Controller**:
  - Implemented `KinematicCharacterBody` using `com.unity.charactercontroller`
  - Created `CharacterData` (config), `CharacterInputData` (input), and `CharacterState` (runtime flags)
  - Added `CharacterInputSystem` for input processing and `CharacterMovementSystem` for physics-based movement
  - Added `CharacterAuthoring` component for entity conversion
- **Input System**:
  - Integrated Unity Input System with `GameInput.inputactions`
  - Implemented actions: Move (WASD/Stick), Jump (Space/South), Sprint (Shift/Stick Press), Crouch (Ctrl/East)
- **Documentation**: Added `CharacterControllerDocumentation.md` and `walkthrough.md`
- **Dependencies**: Added `com.unity.entities`, `com.unity.physics`, `com.unity.charactercontroller`, `com.unity.collections`, and `com.unity.burst`
