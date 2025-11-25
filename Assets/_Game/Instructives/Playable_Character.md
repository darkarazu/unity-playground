# Playable Character Setup Guide

This guide explains how to set up a player-controlled character using MonoBehaviour components in your Unity ECS hybrid project.

## Overview

The player character uses **MonoBehaviour** for simplicity and easy integration with Unity features, while NPCs use **ECS** for performance. A special `PlayerMarker` component bridges the two systems, allowing NPCs to detect the player.

### Architecture

```
Player GameObject:
├── PlayerController (MonoBehaviour) - Handles input & movement
├── PlayerMarker (MonoBehaviour) - Creates ECS entity with PlayerTag
└── CharacterController (Unity built-in) - Physics & collision

NPC Entities (ECS):
└── NPCAISystem detects player via PlayerTag query
```

---

## Quick Setup

### Step 1: Create Player GameObject

1. Create a new **GameObject** in your scene (Right-click Hierarchy → Create Empty)
2. Name it "Player"
3. Position it where you want the player to spawn (e.g., `0, 1, 0`)

### Step 2: Add Character Controller

1. With the Player selected, click **Add Component**
2. Search for "**Character Controller**" (Unity's built-in component)
3. Configure the capsule:
   - **Height**: `2` (adjust to your character size)
   - **Radius**: `0.5`
   - **Center**: `0, 0, 0`

### Step 3: Add PlayerController

1. Click **Add Component** on the Player GameObject
2. Search for "**Player Controller**"
3. Configure movement settings:
   - **Walk Speed**: `5`
   - **Run Speed**: `10`
   - **Crouch Speed**: `2.5`
   - **Jump Height**: `5`
   - **Gravity**: `20`
4. Set **Ground Mask** to include ground layer (usually "Default")

### Step 4: Add PlayerMarker

1. Click **Add Component** on the Player GameObject
2. Search for "**Player Marker**"
3. No configuration needed - it automatically creates the ECS entity!

### Step 5: Add Visual (Optional)

Add a visual representation:
1. Right-click on Player GameObject → **3D Object** → **Capsule**
2. This makes the player visible
3. Disable the capsule's collider (we're using CharacterController for physics)

### Step 6: Test

1. **Play** the scene
2. Use **WASD** to move, **Space** to jump, **Shift** to sprint, **Ctrl** to crouch
3. Check the **Entities Hierarchy** window - you should see a PlayerMarker entity with PlayerTag

---

## Component Details

### PlayerController

**Purpose**: Handles all player input and movement using MonoBehaviour

**Settings:**
- **Movement Speeds**: Control walk, run, and crouch speeds
- **Jump Height**: How high the player jumps
- **Gravity**: Downward force applied when airborne
- **Ground Check Distance**: Raycast distance for ground detection
- **Ground Mask**: Which layers count as ground

**Input (Automatic):**
- WASD / Left Stick → Move
- Space / South Button → Jump
- Left Shift / Left Stick Press → Sprint
- Left Ctrl / East Button → Crouch

### PlayerMarker

**Purpose**: Creates a minimal ECS entity so NPCs can find the player

**How it works:**
- On `Start()`: Creates an entity with `LocalTransform` and `PlayerTag`
- Every `Update()`: Syncs entity position with the GameObject
- On `OnDestroy()`: Cleans up the entity

**Why it's needed:**
NPCs use ECS queries to find the player (`SystemAPI.Query<>().WithAll<PlayerTag>()`). The PlayerMarker ensures the player appears in these queries even though it' s a MonoBehaviour.

---

## Controls

| Input  | Keyboard   | Gamepad                |
| ------ | ---------- | ---------------------- |
| Move   | WASD       | Left Stick             |
| Jump   | Space      | South Button (A/X)     |
| Sprint | Left Shift | Left Stick Press       |
| Crouch | Left Ctrl  | East Button (B/Circle) |

---

## Setting Up the Ground

**Critical:** For both the player and NPCs to detect the ground, it must be in a **subscene** with a **Physics Shape** component.

### Why Subscene is Required

- Player uses Unity's standard physics (CharacterController)
- NPCs use ECS/Unity Physics
- **Physics Shape must be in a subscene to be baked** into ECS physics entities
- Standard colliders work for the player even when in a subscene!

### Step-by-Step Ground Setup

#### 1. Create a Subscene

1. Right-click in Hierarchy → **New Sub Scene** → **Empty Scene...**
2. Name it "Environment" or "EnvironmentSubScene"
3. Save it in `Assets/_Game/Scenes/` or similar

#### 2. Create Ground GameObject

1. Right-click on the **subscene** (in hierarchy) → **3D Object** → **Plane**
2. Name it "Ground"
3. Position at `0, 0, 0`
4. Scale up if needed (e.g., `10, 1, 10`)

#### 3. Add Physics Components

The ground needs **both** physics systems:

1. The Plane already has a **Mesh Collider** (standard Unity physics)
   - ✅ This works for the player's CharacterController
   - Keep this enabled!

2. Add **Physics Shape** component:
   - Click **Add Component** → Search "Physics Shape"
   - Set **Shape Type**: Box
   - Set **Collision Response**: Collide
   - Set **Belongs To**: Everything
   - Set **Collides With**: Everything
   - ✅ This creates an ECS physics entity for NPCs

#### 4. Close the Subscene

1. In Hierarchy, click the **arrow** next to your subscene to collapse it
2. **This triggers baking** - Physics Shape becomes an ECS entity
3. The standard collider remains for GameObject physics

### How It Works

```
Ground (in Subscene)
├── Mesh Renderer (visual)
├── Mesh Filter (visual)
├── Mesh Collider ← Player CharacterController uses this (GameObject physics)
└── Physics Shape ← Bakes to ECS entity for NPCs (ECS physics)
```

**Both systems coexist!**
- Standard collider works at runtime for GameObjects (player)
- Physics Shape creates a baked ECS entity for ECS physics (NPCs)
- They don't conflict - separate physics systems, same GameObject

### Verification

After setup:
- ✅ Player stands on ground (CharacterController + standard collider)
- ✅ NPCs stand on ground (ECS entities + baked Physics Shape)
- ✅ Both can walk on the same surface!

---

## Ground Setup (Important!)

**See the detailed "Setting Up the Ground" section above.** The ground MUST be in a subscene with Physics Shape for NPCs to work!

Quick summary:
1. Create subscene
2. Add Plane to subscene
3. Add Physics Shape component to Plane
4. Close subscene (triggers baking)

---

## Advanced Configuration

### Adjusting Jump Feel

- **Higher jumps**: Increase `Jump Height`
- **Floatier jumps**: Decrease `Gravity`
- **Snappier jumps**: Increase `Gravity`

### Adjusting Movement Feel

- **Faster acceleration**: Movement is instant by default
- **Slower turning**: Decrease the rotation slerp value in `PlayerController.cs` (line ~130)

### Multiple Players

You can have multiple player GameObjects, but:
- Each needs its own `PlayerMarker`
- NPCs will detect the first player they find
- For multiplayer, you'd need to modify NPC AI to target specific players

---

## NPC Integration

### How NPCs Detect the Player

1. `PlayerMarker` creates an entity with `PlayerTag`
2. `NPCAISystem` queries for entities with `PlayerTag`:
   ```csharp
   foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
   ```
3. NPCs get the player's position and calculate distance
4. If within follow range, NPCs chase the player

**No manual setup required** - it just works! 🎯

---

## Troubleshooting

### Player Falls Through Ground
- ✅ Ensure ground has a collider (planes have box colliders by default)
- ✅ Check `Ground Mask` includes the ground GameObject's layer
- ✅ Verify CharacterController is enabled

### No Input Response
- ✅ Check PlayerController component is attached and enabled
- ✅ Look for errors in Console
- ✅ Verify input actions are enabled (should be automatic)

### NPCs Don't Detect Player
- ✅ Ensure `PlayerMarker` component is attached
- ✅ Check Entities Hierarchy window for PlayerMarker entity
- ✅ Verify entity has `PlayerTag` component
- ✅ Check Console for PlayerMarker debug messages

### Player Gets Stuck on Edges
- ✅ Adjust CharacterController `Step Offset` (try `0.3`)
- ✅ Increase `Slope Limit` if needed (default `45`)

### Character Controller Not Found Error
- ✅ Make sure you added Unity's Character Controller component
- ✅ It should show up under "Character" in Add Component menu

---

## Hybrid Architecture Benefits

✅ **Simple Player Code**: Traditional MonoBehaviour workflow  
✅ **Easy Unity Integration**: Animations, UI, audio work normally  
✅ **NPC Performance**: ECS handles many NPCs efficiently  
✅ **Best of Both Worlds**: Use the right tool for each job  

---

## What's Next?

- **Add NPCs**: See [NPC_AI.md](file:///d:/Unity%20Projects/My%20project/Assets/_Game/Instructives/NPC_AI.md) for NPC setup
- **Add Camera Follow**: Create a camera script that follows the player
- **Add Animations**: Attach an Animator component to the visual capsule
- **Add Audio**: Play footstep sounds based on `IsGrounded` state

---

## Technical Notes

### Why Not Full ECS for Player?

ECS excels at processing many similar entities but adds complexity for a single character. Benefits of MonoBehaviour for player:
- Easier to debug and extend
- Direct access to Unity features (Animator, Audio, etc.)
- Simpler code reviews for gameplay programmers
- Faster iteration for player-specific features

### PlayerMarker Performance

Minimal overhead:
- Creates one entity (virtually free)
- Updates one `LocalTransform` per frame (~microseconds)
- Small price for clean architecture separation

### Extending PlayerController

The `PlayerController.cs` script is designed to be easily extended:
- Add public methods for special moves
- Subscribe to events for state changes
- Access `IsGrounded`, `IsSprinting`, etc. properties
- Modify movement code for wall-running, dashing, etc.

Happy coding! 🎮
