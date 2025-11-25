# Third Person Camera System Guide

Complete guide for the WoW-style 3rd person camera system using Cinemachine 3.

## Overview

The camera system provides smooth, professional 3rd person controls with:
- **Left Mouse Button (LMB)**: Orbit camera around player (view only)
- **Right Mouse Button (RMB)**: Rotate player to face camera direction
- **Scroll Wheel**: Zoom in/out

**Architecture:**
- **Cinemachine FreeLook Camera**: Industry-standard orbital camera
- **CameraController**: Handles mouse input, zoom, and player rotation
- **CinemachineBrain**: On Main Camera, blends virtual cameras

---

## Prerequisites

### 1. Install Cinemachine Package

> [!IMPORTANT]
> You must install Cinemachine before proceeding.

1. **Window** → **Package Manager**
2. Select **Unity Registry**
3. Search for "**Cinemachine**"
4. Click **Install** (Cinemachine 3.x for Unity 6)

---

## Setup Instructions

### Step 1: Setup Main Camera (CinemachineBrain)

**CRITICAL:** Virtual cameras need a brain on your Main Camera.

1. Select **Main Camera** in Hierarchy
2. **Add Component** → Search "**Cinemachine Brain**"
3. Add it - default settings are fine

✅ Your Main Camera now has CinemachineBrain

### Step 2: Create FreeLook Camera

1. **GameObject** → **Cinemachine** → **FreeLook Camera**
2. A new GameObject "FreeLook Camera" appears in Hierarchy

### Step 3: Configure FreeLook Camera

**Select FreeLook Camera** in Hierarchy.

#### A. Set Tracking Target

In **CinemachineCamera** component:

1. Find **Tracking Target** field (below Lens)
2. Drag your **Player** GameObject into it
3. ⚠️ Warning disappears when set

#### B. Configure Orbital Follow

Expand **Cinemachine Orbital Follow**:

1. **Binding Mode**: `World Space` (should be default)
2. **Position Damping**: X: 1, Y: 1, Z: 1
3. **Orbit Style**: `Three Ring`
4. Configure rigs (click each to expand):
   - **Top**: Height `5`, Radius `2`
   - **Center**: Height `2.25`, Radius `4`
   - **Bottom**: Height `0.1`, Radius `2.5`
5. **Spline Curvature**: `0.5`

#### C. Configure Rotation Composer

Expand **Cinemachine Rotation Composer**:

1. **Screen Position**: X: 0, Y: 0
2. **Target Offset**: X: 0, Y: 0, Z: 0
3. **Damping**: X: 0.5, Y: 0.5

#### D. Disable Input Axis Controller

**CRITICAL:** We control input manually.

Expand **Cinemachine Input Axis Controller**:

**Option 1** (Recommended):
- **Uncheck** the component checkbox (top-left)

**Option 2**:
- Expand **Driven Axes**
- Uncheck all: Look Orbit X, Look Orbit Y, Orbit Scale

#### E. Optional: Adjust Lens

In **CinemachineCamera**:
1. Expand **Lens**
2. **field of view**: `60` (or adjust 50-70 for preference)

### Step 4: Add CameraController to Player

1. Select your **Player** GameObject
2. **Add Component** → Search "**Camera Controller**"
3. In CameraController component:
   - **Free Look Camera**: Drag the **FreeLook Camera** GameObject here
   - **Mouse Sensitivity**: `8` (adjust to preference)
   - **Zoom Sensitivity**: `5` (adjust scroll wheel speed)
   - **Player Rotation Speed**: `10` (smooth rotation speed when RMB held)
   - **Invert Y**: Check if you prefer inverted vertical

✅ Camera system is now configured

---

## How It Works

### Camera Controls

```
Mouse Input → CameraController
              ├─ LMB held → Orbit Cinemachine camera (view only)
              ├─ RMB held → Orbit camera + Rotate player
              └─ Scroll Wheel → Zoom in/out (adjust all rig radii)
```

### LMB (Orbit View)

- Camera orbits around player
- Player keeps current facing direction
- **Use case**: Look around without turning character

### RMB (Rotate Player)

- Camera orbits around player
- **Player rotates** smoothly to face camera direction
- Uses Quaternion.Slerp for natural turning
- Rotation speed configurable (default: 10)
- **Use case**: Turn character to face a direction

### Scroll Wheel (Zoom)

- Scroll up: Zoom in (closer to player)
- Scroll down: Zoom out (further from player)
- **Proportional Scaling**: Adjusts radius AND height of all orbital rigs
- Maintains original orbital shape (sphere-like)
- Camera distance stays consistent when orbiting vertically
- Clamped by **Min Zoom** and **Max Zoom** settings

### Dynamic FOV (Field of View)

- **Normal FOV**: Standard field of view during regular movement
- **Sprint FOV**: Wider field of view when sprinting AND moving
- Smooth transitions between normal and sprint FOV
- Enhances perception of speed when running
- Only activates when player is actually moving (not just holding Shift)

---

## Camera Controls Reference

| Input            | Action                    |
| ---------------- | ------------------------- |
| **LMB + Mouse**  | Orbit camera (view only)  |
| **RMB + Mouse**  | Rotate player with camera |
| **Scroll Wheel** | Zoom in/out               |

---

## Customization

### Camera Distance

Edit rig **Radius** values in **Orbital Follow**:
- Smaller radius = closer camera
- Larger radius = further camera
- Different values per rig = dynamic distance based on angle

### Camera Height

Edit rig **Height** values in **Orbital Follow**:
- Top rig: High angle (bird's eye)
- Center rig: Over-shoulder
- Bottom rig: Low angle (looking up)

### Mouse Sensitivity

In **CameraController** component on Player:
- **Mouse Sensitivity**: Higher = faster camera rotation
- Typical range: 1-5

### Invert Vertical Axis

In **CameraController** component:
- **Invert Y**: Check to invert mouse Y-axis

### Zoom Settings

In **CameraController** component on Player:
- **Zoom Sensitivity**: Higher = faster zoom with scroll wheel (default: 5)
- **Min Zoom**: Closest camera distance (default: 2)
- **Max Zoom**: Furthest camera distance (default: 8)
- Typical range: 1-15 units
- Zoom uses proportional scaling (maintains orbital sphere shape)

### FOV Settings

In **CameraController** component on Player:
- **Normal FOV**: Field of view during normal movement (default: 60)
- **Sprint FOV**: Field of view when sprinting (default: 70)
- **FOV Transition Speed**: How fast FOV changes (default: 5)
- Higher FOV = wider view (sense of speed)
- Smoothly transitions based on sprint state AND movement
- FOV only widens when player is moving + sprinting

### Player Rotation Settings

In **CameraController** component on Player:
- **Player Rotation Speed**: How fast player rotates when RMB is held (default: 10)
- Higher = faster rotation, Lower = slower/smoother rotation
- Uses Quaternion.Slerp for natural turning
- Applied only when RMB is held

---

## Troubleshooting

### Camera Doesn't Follow Player
- ✅ Check **Tracking Target** is set in CinemachineCamera
- ✅ Check **CinemachineBrain** is on Main Camera
- ✅ Check FreeLook Camera is enabled and Priority is high enough

### Camera Doesn't Respond to Mouse
- ✅ Check **CameraController** component is on Player
- ✅ Check **Free Look Camera** field is set in CameraController
- ✅ Check **Input Axis Controller** is disabled on FreeLook Camera
- ✅ Check console for errors

### Player Doesn't Rotate with RMB
- ✅ Check **PlayerController** script was updated (has `SetYRotation` method)
- ✅ Check both PlayerController and CameraController are on Player
- ✅ Recompile scripts if needed

### Movement Feels Wrong
- Old scripts cached? Recompile:
  - **Assets** → **Reimport All**
  - Or restart Unity Editor

### Camera Clips Through Walls
Add **Cinemachine Deoccluder**:
1. Select FreeLook Camera
2. **Add Component** → "Cinemachine Deoccluder"
3. Set **Collision Filter** to ground/wall layers

---

## Architecture Notes

### Why Separate CameraController?

**Single Responsibility Principle:**
- **PlayerController**: Movement logic
- **CameraController**: Camera/rotation logic
- Clean separation, easy to test and modify

### Why Cinemachine?

- ✅ Industry standard for cameras
- ✅ Smooth orbital movement with three rigs
- ✅ Built-in features (damping, collision, etc.)
- ✅ Easy to extend (zoom, FOV, shake, etc.)

### Cinemachine 3 Changes

Unity 6 uses Cinemachine 3.x with updated API:
- `Follow`/`Look At` → **Tracking Target**
- Separate components → **Procedural Components**
- More modular, component-based architecture

---

## Common Modifications

### Lock Camera Behind Player

Set **Recentering** in **Orbital Follow**:
- Enable Y-Axis recentering
- Set wait time (e.g., 2 seconds)
- Camera returns behind player automatically

### Add Camera Shake

1. Add **Cinemachine Basic Multi Channel Perlin** to FreeLook Camera
2. Configure noise profile
3. Trigger shake when needed (landing, damage, etc.)

---

## What's Next?

- **Animations**: Add Animator component, blend based on `PlayerController.IsSprinting`, etc.
- **Collision Avoidance**: Add Deoccluder for wall clipping
- **Camera Shake**: On jump landing, explosions, etc.

Happy gaming! 🎮
