# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
