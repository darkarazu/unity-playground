# Unity ECS Project

Welcome to the Unity ECS Project! This project is built using Unity's Entity Component System (ECS) to leverage high-performance data-oriented design.

## Versioning
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Changelog

### [0.3.0] - 2025-11-25
**Hybrid Architecture - MonoBehaviour Player**

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
  - Cleans up entity on Destroy
- **Documentation**: `Playable_Character.md` comprehensive setup guide

#### Removed
- **CharacterInputSystem**: No longer needed for player (NPCs still use their own AI input)
- **ECS Player Complexity**: Eliminated ECS overhead for single player character

#### Technical Notes
- NPCs unchanged - still use ECS and detect player via PlayerTag
- Clean separation: GameObject player, Entity NPCs
- Minimal performance overhead (one entity update per frame)
- Easy to extend player with animations, UI, audio

### [0.2.0] - 2025-11-25
**NPC AI System Implementation**

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

### [0.1.0] - 2025-11-24
**Initial Release - ECS Character Controller Prototype**

#### Added
- **Project Structure**: Established `Assets/_Game/` directory structure separating user content from third-party assets.
- **ECS Architecture**: Implemented core ECS folder structure (`Components`, `Systems`, `Authoring`).
- **Character Controller**:
    - Implemented `KinematicCharacterBody` using `com.unity.charactercontroller`.
    - Created `CharacterData` (config), `CharacterInputData` (input), and `CharacterState` (runtime flags).
    - Added `CharacterInputSystem` for input processing and `CharacterMovementSystem` for physics-based movement.
    - Added `CharacterAuthoring` component for entity conversion.
- **Input System**:
    - Integrated Unity Input System with `GameInput.inputactions`.
    - Implemented actions: Move (WASD/Stick), Jump (Space/South), Sprint (Shift/Stick Press), Crouch (Ctrl/East).
- **Documentation**: Added `CharacterControllerDocumentation.md` and `walkthrough.md`.
- **Dependencies**: Added `com.unity.entities`, `com.unity.physics`, `com.unity.charactercontroller`, `com.unity.collections`, and `com.unity.burst`.

## Getting Started

### Prerequisites
- Unity 6
- ECS packages installed (see Changelog)
- **Important:** Unity Physics "Physics Custom" sample must be imported (see Setup step 1)

### Initial Setup Required

> [!IMPORTANT]
> Before creating characters, you must import the Physics samples to access the Physics Shape components.

#### 0. Import Physics Samples (Required)
1. Open **Window** → **Package Manager**
2. Find **"Physics"** package in the list
# Unity ECS Project

Welcome to the Unity ECS Project! This project is built using Unity's Entity Component System (ECS) to leverage high-performance data-oriented design.

## Versioning
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Changelog

### [0.3.0] - 2025-11-25
**Hybrid Architecture - MonoBehaviour Player**

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
  - Cleans up entity on Destroy
- **Documentation**: `Playable_Character.md` comprehensive setup guide

#### Removed
- **CharacterInputSystem**: No longer needed for player (NPCs still use their own AI input)
- **ECS Player Complexity**: Eliminated ECS overhead for single player character

#### Technical Notes
- NPCs unchanged - still use ECS and detect player via PlayerTag
- Clean separation: GameObject player, Entity NPCs
- Minimal performance overhead (one entity update per frame)
- Easy to extend player with animations, UI, audio

### [0.2.0] - 2025-11-25
**NPC AI System Implementation**

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

### [0.1.0] - 2025-11-24
**Initial Release - ECS Character Controller Prototype**

#### Added
- **Project Structure**: Established `Assets/_Game/` directory structure separating user content from third-party assets.
- **ECS Architecture**: Implemented core ECS folder structure (`Components`, `Systems`, `Authoring`).
- **Character Controller**:
    - Implemented `KinematicCharacterBody` using `com.unity.charactercontroller`.
    - Created `CharacterData` (config), `CharacterInputData` (input), and `CharacterState` (runtime flags).
    - Added `CharacterInputSystem` for input processing and `CharacterMovementSystem` for physics-based movement.
    - Added `CharacterAuthoring` component for entity conversion.
- **Input System**:
    - Integrated Unity Input System with `GameInput.inputactions`.
    - Implemented actions: Move (WASD/Stick), Jump (Space/South), Sprint (Shift/Stick Press), Crouch (Ctrl/East).
- **Documentation**: Added `CharacterControllerDocumentation.md` and `walkthrough.md`.
- **Dependencies**: Added `com.unity.entities`, `com.unity.physics`, `com.unity.charactercontroller`, `com.unity.collections`, and `com.unity.burst`.

## Getting Started

### Prerequisites
- Unity 6
- ECS packages installed (see Changelog)
- **Important:** Unity Physics "Physics Custom" sample must be imported (see Setup step 1)

### Initial Setup Required

> [!IMPORTANT]
> Before creating characters, you must import the Physics samples to access the Physics Shape components.

#### 0. Import Physics Samples (Required)
1. Open **Window** → **Package Manager**
2. Find **"Physics"** package in the list
3. Click on it, then go to the **Samples** tab
4. Click **Import** next to **"Physics Custom"**
5. This adds the Physics Shape authoring components needed for ECS

### Setting Up the Player Character (v0.3.0+)

**The player now uses MonoBehaviour for simplicity!**

### Troubleshooting

**General:**
- **"Physics Shape" component not found**: Import the "Physics Custom" sample from the Unity Physics package
- **No entities visible**: Check the Entities Hierarchy window while in Play Mode

**Player (v0.3.0+):**
- **Player falls through ground**: Ground MUST be in a subscene with Physics Shape component
- **Player doesn't move**: Ensure PlayerController and CharacterController components are added
- **No input response**: Check PlayerController is enabled

**NPCs (v0.2.0+):**
- **NPCs fall through ground**: Ground must be in a subscene with Physics Shape component
- **NPCs don't follow player**: Ensure player has PlayerMarker component
- **NPCs don't move**: NPCs must be in a subscene with CharacterAuthoring + NPCAuthoring

**Hybrid Architecture:**
- **EntityManager errors on exit**: Fixed in v0.3.0+ (PlayerMarker checks World.IsCreated)
- **NPCs can't detect player**: Player needs PlayerMarker component to create ECS entity with PlayerTag

#### Quick Setup:
1. Create a GameObject named "Player"
2. Add `CharacterController` component (Unity built-in)
- **Character falls through ground**: Add a plane with Physics Shape component to the SubScene
- **No entities visible**: Check the Entities Hierarchy window while in Play Mode
