# Unity ECS Hybrid Project

A Unity 6 project demonstrating **hybrid architecture** with MonoBehaviour player character and ECS-powered NPCs.

## 🎯 Project Overview

This project showcases best practices for using Unity's Entity Component System (ECS) alongside traditional MonoBehaviour workflows. The architecture uses:

- **MonoBehaviour** for the player character (simple, familiar, easy to extend)
- **ECS** for NPC AI and systems (high performance, scalable)
- **Shared Physics** ground setup that works with both systems

## ✨ Features

### Player Controller (MonoBehaviour)
- MMORPG-style 3rd person camera with Cinemachine
- Third-person character movement using Unity's built-in `CharacterController`
- Strafe movement: W/S forward/back, A/D left/right
- LMB: Orbit camera (view only), RMB: Rotate player with camera
- Input handling: jumping, sprinting (forward/strafe only), crouching
- **Full controller support**: PlayStation, Xbox, and Nintendo Switch gamepads
  - Left Stick → Movement, Right Stick → Camera orbit
  - D-Pad → Progressive zoom in/out
  - See `Controller.md` for complete button mappings
- Easy to extend with animations, UI, and Unity features

### NPC AI System (ECS)
- **Intelligent patrol behavior**: NPCs roam within configurable areas
  - Randomized starting positions and patrol points
  - Varied wait times at patrol points (min/max range)
  - Terrain-aware pathfinding (raycasts to find surface height)
- **Smart follow behavior**: NPCs detect and chase the player
  - Follow timeout system (prevents endless chasing of unreachable targets)
  - Return-to-origin: NPCs return to where they started following
- **Proper state machine**: NPCs ignore player completely while returning
- Fully Burst-compiled and job-scheduled for performance
- Visual debug gizmos for tuning AI parameters

### Camera System (Cinemachine)
- **Professional 3rd person camera** using Cinemachine 3.x
- **LMB + Mouse / Right Stick**: Orbit camera around player
- **RMB + Mouse**: Smoothly rotate player to face camera
- **Scroll Wheel / D-Pad**: Proportional zoom (maintains orbital shape)
- **Dynamic FOV**: Widen field of view when sprinting
- **Rotation catch-up**: Player smoothly rotates to camera direction after input
- Configurable sensitivity (separate for mouse and gamepad), transitions, and camera distance

### Hybrid Architecture
- **PlayerMarker** component bridges MonoBehaviour player with ECS NPCs
- NPCs detect player using ECS queries (`PlayerTag`)
- Ground works for both physics systems (standard colliders + Physics Shape)
- Clean separation of concerns

## 📦 Dependencies

- **Unity 6** (required)
- **Cinemachine 3.x** (`com.unity.cinemachine`) - **NEW!**
- **Unity ECS** (`com.unity.entities`)
- **Unity Physics** (`com.unity.physics`)
- **Unity Character Controller** (`com.unity.charactercontroller`)
- **Unity Input System** (`com.unity.inputsystem`)
- **Unity Collections** (`com.unity.collections`)
- **Unity Burst** (`com.unity.burst`)

## 🚀 Quick Start

### 1. Import Physics Samples (Required)

> [!IMPORTANT]
> You must import the Physics Custom sample to access Physics Shape components.

1. Open **Window** → **Package Manager**
2. Find **"Physics"** package
3. Go to **Samples** tab
4. Click **Import** next to **"Physics Custom"**

### 2. Set Up Ground (Plane or Terrain)

You can use either a simple Plane or a full Unity Terrain:

**Option A: Simple Plane (Prototyping)**
1. Create a subscene (Right-click Hierarchy → New Sub Scene)
2. Add a Plane inside the subscene
3. Add **Physics Shape** component (Shape Type: Box)
4. Close subscene to trigger baking

**Option B: Unity Terrain (Production)**
1. Create a Terrain in the **Main Scene** (NOT subscene)
2. Add **Terrain Collider** component
3. Add **Terrain Physics Runtime** component
   - This automatically generates ECS colliders at runtime
   - Works for both Player (MonoBehaviour) and NPCs (ECS)

**See full guide:** **[Terrain.md](Assets/_Game/Instructives/Terrain.md)**

### 3. Setup Camera System (NEW!)

1. Install **Cinemachine** package (Window → Package Manager)
2. Select **Main Camera** → Add **Cinemachine Brain** component
3. Create **FreeLook Camera** (GameObject → Cinemachine → FreeLook Camera)
4. Configure camera (see **[Camera.md](Assets/_Game/Instructives/Camera.md)** for detailed steps)

### 4. Create Player

1. Create GameObject named "Player"
2. Add components:
   - `CharacterController` (Unity built-in)
   - `PlayerController` (handles movement)
   - `PlayerMarker` (creates ECS entity for NPC detection)
   - `CameraController` (handles camera and rotation) - **NEW!**
3. In CameraController:
   - Drag FreeLook Camera to **Free Look Camera** field
4. Optional: Add visual mesh (Capsule)

**See full guide:** **[Playable_Character.md](Assets/_Game/Instructives/Playable_Character.md)** and **[Camera.md](Assets/_Game/Instructives/Camera.md)**

### 5. Create NPCs

1. Create GameObject in the subscene
2. Add components:
   - `CharacterAuthoring` (ECS movement)
   - `NPCAuthoring` (AI behavior)
3. Configure patrol radius and follow settings in Inspector

**See full guide:** **[NPC_AI.md](Assets/_Game/Instructives/NPC_AI.md)**

## 🎮 Controls

| Action            | Input                   | Description                      |
| ----------------- | ----------------------- | -------------------------------- |
| **Movement**      | WASD / Left Stick       | Strafe (Forward/Back/Left/Right) |
| **Orbit Camera**  | LMB + Mouse             | Rotate camera around player      |
| **Rotate Player** | RMB + Mouse             | Rotate player to face camera     |
| **Zoom Camera**   | Scroll Wheel            | Zoom in/out                      |
| **Jump**          | Space / Button South    | Jump                             |
| **Sprint**        | Left Shift / L3         | Run faster                       |
| **Crouch**        | Left Ctrl / Button East | Crouch (not yet implemented)     |

## 📁 Project Structure

```
Assets/_Game/
├── Scenes/                  # Main scenes and subscenes
├── Scripts/
│   ├── Player/             # MonoBehaviour player components
│   │   ├── PlayerController.cs
│   │   ├── CameraController.cs    # Camera logic
│   │   └── PlayerMarker.cs
│   ├── Components/         # ECS components
│   │   ├── Character/      # Movement components
│   │   ├── Input/          # Input data components
│   │   ├── AI/             # NPC AI components
│   │   └── Tags/           # PlayerTag, NPCTag
│   ├── Systems/            # ECS systems
│   │   ├── Simulation/     # NPCAISystem, CharacterMovementSystem
│   │   └── Initialization/ # CharacterInputSystem
│   ├── Authoring/          # Baking components
│   │   ├── CharacterAuthoring.cs  # NPC entities
│   │   └── NPCAuthoring.cs        # NPC AI configuration
│   ├── Hybrid/             # Hybrid ECS/MonoBehaviour bridges
│   │   └── TerrainPhysicsRuntime.cs # Runtime terrain collider generation
│   └── Character/          # Character processor (ECS movement logic)
└── Instructives/           # Documentation
    ├── Playable_Character.md
    ├── NPC_AI.md
    ├── Camera.md
    ├── Controller.md
    └── Terrain.md
```

## 🏗️ Architecture Highlights

### Hybrid Approach

```
Player (MonoBehaviour):
PlayerController → CharacterController → Standard Physics

NPCs (ECS):
NPCAISystem → CharacterInputData → CharacterMovementSystem → ECS Physics

Bridge:
PlayerMarker → Creates ECS entity with PlayerTag → NPCs query for player
```

### Why Hybrid?

- **Player**: One character, needs frequent changes, animations, UI integration → MonoBehaviour wins
- **NPCs**: Many entities, performance-critical, data-oriented → ECS wins
- **Best of both worlds**: Use the right tool for each job

## 📖 Documentation

- **[Playable_Character.md](Assets/_Game/Instructives/Playable_Character.md)**: Complete player setup guide
- **[NPC_AI.md](Assets/_Game/Instructives/NPC_AI.md)**: NPC AI behavioral system guide
- **[Camera.md](Assets/_Game/Instructives/Camera.md)**: Camera system setup and configuration
- **[Controller.md](Assets/_Game/Instructives/Controller.md)**: Controller input mappings and setup
- **[Terrain.md](Assets/_Game/Instructives/Terrain.md)**: Terrain physics setup guide
- **[CHANGELOG.md](CHANGELOG.md)**: Detailed version history

## 🔧 Troubleshooting

### Player Issues
- **Falls through ground**: Ground must be in subscene with Physics Shape component
- **No movement**: Ensure PlayerController and CharacterController are added
- **No input**: Check PlayerController component is enabled

### NPC Issues
- **Fall through ground**: Ground must be in subscene with Physics Shape
- **Don't follow player**: Player needs PlayerMarker component
- **Don't move**: NPCs must be in subscene with CharacterAuthoring + NPCAuthoring

### General
- **Physics Shape not found**: Import "Physics Custom" sample from Package Manager
- **EntityManager errors on exit**: Fixed in v0.3.0+ (PlayerMarker checks World.IsCreated)

## 🎓 Learning Resources

This project demonstrates:
- ✅ Unity 6 ECS hybrid architecture
- ✅ MonoBehaviour and ECS coexistence
- ✅ Cinemachine 3.x professional camera system
- ✅ Dynamic entity queries (PlayerTag detection)
- ✅ Subscene baking workflow
- ✅ Burst compilation and job scheduling
- ✅ Unity Input System integration (Gamepad + KBM)
- ✅ Runtime Terrain Physics generation

## 📝 Version

**Current Version:** 0.6.0

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

**Roadmap**

See [ROADMAP.md](ROADMAP.md) for detailed version history.

## 🤝 Contributing

This project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

**Built with Unity 6 ECS** | **Hybrid MonoBehaviour + ECS Architecture**
