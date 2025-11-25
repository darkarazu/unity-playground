# Unity ECS Project

Welcome to the Unity ECS Project! This project is built using Unity's Entity Component System (ECS) to leverage high-performance data-oriented design.

## Versioning
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Changelog

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

### Setting Up the Character Controller

#### 1. Create a SubScene
SubScenes are required for ECS baking - they convert GameObjects with authoring components into entities.

- Right-click in Hierarchy → **New Sub Scene** → **Empty Scene...**
- Name it `CharacterSubScene`
- Save it in `Assets/_Game/Scenes/`

#### 2. Create the Character
- Right-click in the SubScene → **3D Object** → **Capsule**
- Add the **CharacterAuthoring** component (`Add Component` → Search for "Character Authoring")
- Configure speeds and jump height in the Inspector

#### 3. Add Physics Components
The Character Controller requires Unity Physics collision:

- With the Capsule selected, click **Add Component**
- Search for **"Physics Shape"** (available after importing Physics Custom sample)
- The component should appear - add it
- Set **Shape Type** to **Capsule**
- Set **Collision Response** to **Collide**
- Adjust capsule dimensions if needed

> [!NOTE]
> If "Physics Shape" doesn't appear, make sure you imported the "Physics Custom" sample (see step 0).

#### 4. Create Ground Plane
The character needs a surface to stand on:

- Right-click in the SubScene → **3D Object** → **Plane**
- Add **Physics Shape** component to the Plane
- Set **Shape Type** to **Box**
- Set **Collision Response** to **Collide**
- Position at `(0, 0, 0)`

#### 5. Setup the Camera
For testing purposes:

- Select the Main Camera (should be outside the SubScene)
- Set Position to `(0, 2, -10)`
- Set Rotation to look at the character

#### 6. Test the Setup
- Enter Play Mode
- Open **Window** → **Entities** → **Hierarchy** to verify the entity was created
- Open **Window** → **Entities** → **Inspector** to see the entity's components
- Use **WASD** to move, **Space** to jump, **Shift** to sprint, **Ctrl** to crouch

### Controls
- **WASD** / **Left Stick**: Move
- **Space** / **South Button (A/X)**: Jump
- **Left Shift** / **Left Stick Press**: Sprint
- **Left Ctrl** / **East Button (B/Circle)**: Crouch

### NPC AI System (v0.2.0+)
NPCs with patrol, follow, and return behaviors are now supported! 

**Quick Setup:**
1. Create a GameObject with both `CharacterAuthoring` and `NPCAuthoring` components
2. Set `Is Player Controlled` to **false** on `CharacterAuthoring`
3. Configure patrol radius and follow distances in `NPCAuthoring`
4. NPCs automatically detect and follow the player using `PlayerTag`

**See full documentation:** `Assets/_Game/Instructives/NPC_AI.md`

### Troubleshooting
- **"Physics Shape" component not found**: Import the "Physics Custom" sample from the Unity Physics package
- **No movement**: Ensure the character is in a SubScene and has a Physics Shape component
- **Character falls through ground**: Add a plane with Physics Shape component to the SubScene
- **No entities visible**: Check the Entities Hierarchy window while in Play Mode
