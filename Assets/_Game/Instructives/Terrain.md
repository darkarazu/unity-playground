# Terrain Setup for Hybrid ECS

This guide explains how to set up a Unity Terrain that supports collision for both the MonoBehaviour Player and ECS NPCs.

## Overview

- **Player**: Uses Unity's standard `Terrain Collider` (works natively with `CharacterController`).
- **NPCs**: Uses a custom `TerrainPhysicsRuntime` script to generate an ECS `PhysicsCollider` at runtime.

## Prerequisites
- Unity 6
- `com.unity.physics` package installed.

## Step-by-Step Setup

### 1. Create the Terrain (Main Scene)
**Important:** Do NOT put the Terrain in a Subscene. It must be in the **Main Scene** to be visible and work with the Player.

1.  Right-click in Hierarchy (Main Scene) → **3D Object** → **Terrain**.
2.  (Optional) Import your heightmap via Terrain Settings (Gear icon) → Import Raw.

### 2. Configure Physics

#### A. Terrain Collider (Native)
- **Status**: Should be added automatically when you create the Terrain.
- **Purpose**: Handles collision for the **Player** (CharacterController).
- **Action**: Verify it is present and enabled on the Terrain GameObject.

#### B. ECS Physics (Runtime Generation)
Since the Terrain is in the Main Scene (not baked), we generate the ECS collider at runtime.

1.  **Add Component**:
    - Select your Terrain GameObject.
    - Click **Add Component** → Search for **Terrain Physics Runtime**.
    - **Assign Terrain**: Drag the Terrain component (from the same GameObject) into the **Terrain** field of the script (or leave it empty to auto-detect).

**Note**: The script is located at `Assets/_Game/Scripts/Hybrid/TerrainPhysicsRuntime.cs`.

## Troubleshooting

### Terrain Invisible in Play Mode
- ✅ **Ensure terrain is NOT in a Subscene** - It must be in the Main Scene
- ✅ **Check terrain is enabled** - Verify it's not disabled in the Inspector
- ✅ **Verify materials** - Ensure terrain has valid materials assigned

### NPCs Fall Through Terrain
- ✅ **Check Console** - Look for the green success message from `TerrainPhysicsRuntime`
- ✅ **Verify component is attached** - Terrain should have `TerrainPhysicsRuntime` component
- ✅ **Check Start() was called** - If no console message, the script didn't run
- ✅ **Verify terrain reference** - If you manually assigned the Terrain field, ensure it's correct
- ✅ **Check Physics Debugger** - Window → Analysis → Physics Debugger to visualize colliders

### Common Issues
- **"No Terrain found!" error** - The script can't find the Terrain component
- **No console message** - The `TerrainPhysicsRuntime` component is not attached or disabled
- **Terrain in Subscene** - Move it to the Main Scene (subscenes hide the visual terrain)

## Verification
1.  **Play** the scene.
2.  **Check Console** - You should see a green success message with terrain details.
3.  **Player**: Walk on the terrain. You should stand on the surface.
4.  **NPCs**: NPCs should also walk on the terrain surface.
5.  **Debug**: Open **Window** → **Analysis** → **Physics Debugger** to see the collision meshes.
