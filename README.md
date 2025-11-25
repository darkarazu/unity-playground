# Unity ECS Project

Welcome to the Unity ECS Project! This project is built using Unity's Entity Component System (ECS) to leverage high-performance data-oriented design.

## Versioning
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Changelog

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

### Troubleshooting
- **"Physics Shape" component not found**: Import the "Physics Custom" sample from the Unity Physics package
- **No movement**: Ensure the character is in a SubScene and has a Physics Shape component
- **Character falls through ground**: Add a plane with Physics Shape component to the SubScene
- **No entities visible**: Check the Entities Hierarchy window while in Play Mode
