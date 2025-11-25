# Unity ECS Hybrid Project

A Unity 6 project demonstrating **hybrid architecture** with MonoBehaviour player character and ECS-powered NPCs.

## 🎯 Project Overview

This project showcases best practices for using Unity's Entity Component System (ECS) alongside traditional MonoBehaviour workflows. The architecture uses:

- **MonoBehaviour** for the player character (simple, familiar, easy to extend)
- **ECS** for NPC AI and systems (high performance, scalable)
- **Shared Physics** ground setup that works with both systems

## ✨ Features

### Player Controller (MonoBehaviour)
- Third-person character movement using Unity's built-in `CharacterController`
- Input handling: WASD movement, jumping, sprinting, crouching
- Supports both keyboard/mouse and gamepad controls
- Easy to extend with animations, UI, and Unity features

### NPC AI System (ECS)
- **Patrol behavior**: NPCs roam within configurable areas
- **Follow behavior**: NPCs detect and chase the player
- **Return behavior**: NPCs return to patrol area when too far from center
- Fully Burst-compiled and job-scheduled for performance
- Visual debug gizmos for tuning AI parameters

### Hybrid Architecture
- **PlayerMarker** component bridges MonoBehaviour player with ECS NPCs
- NPCs detect player using ECS queries (`PlayerTag`)
- Ground works for both physics systems (standard colliders + Physics Shape)
- Clean separation of concerns

## 📦 Dependencies

- **Unity 6** (required)
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

### 2. Set Up Ground

Critical for hybrid architecture:

1. Create a subscene (Right-click Hierarchy → New Sub Scene)
2. Add a Plane inside the subscene
3. Add **Physics Shape** component (Shape Type: Box)
4. Close subscene to trigger baking

**Why subscene?** Physics Shape must be baked to work with ECS physics, while standard colliders work for MonoBehaviour player.

### 3. Create Player

1. Create GameObject named "Player"
2. Add components:
   - `CharacterController` (Unity built-in)
   - `PlayerController` (handles movement)
   - `PlayerMarker` (creates ECS entity for NPC detection)
3. Optional: Add visual mesh (Capsule)

**See full guide:** `Assets/_Game/Instructives/Playable_Character.md`

### 4. Create NPCs

1. Create GameObject in the subscene
2. Add components:
   - `CharacterAuthoring` (ECS movement)
   - `NPCAuthoring` (AI behavior)
3. Configure patrol radius and follow settings in Inspector

**See full guide:** `Assets/_Game/Instructives/NPC_AI.md`

## 🎮 Controls

| Action           | Keyboard   | Gamepad                |
| ---------------- | ---------- | ---------------------- |
| Move             | WASD       | Left Stick             |
| Jump             | Space      | South Button (A/X)     |
| Sprint           | Left Shift | Left Stick Press       |
| Crouch :warning: | Left Ctrl  | East Button (B/Circle) |

:warning: Not yet implemented

## 📁 Project Structure

```
Assets/_Game/
├── Scenes/                  # Main scenes and subscenes
├── Scripts/
│   ├── Player/             # MonoBehaviour player components
│   │   ├── PlayerController.cs
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
│   └── Character/          # Character processor (ECS movement logic)
└── Instructives/           # Documentation
    ├── Playable_Character.md
    └── NPC_AI.md
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
- ✅ Dynamic entity queries (PlayerTag detection)
- ✅ Subscene baking workflow
- ✅ Burst compilation and job scheduling
- ✅ Unity Input System integration
- ✅ Kinematic Character Controller usage

## 📝 Version

**Current Version:** 0.3.0

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## 🤝 Contributing

This project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

**Built with Unity 6 ECS** | **Hybrid MonoBehaviour + ECS Architecture**
