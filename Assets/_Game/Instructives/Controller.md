# Controller Input Guide

This guide explains controller support for the game, including PlayStation, Xbox, and Nintendo Switch controllers. The game uses Unity's **Input System** with programmatic bindings for maximum flexibility and cross-platform compatibility.

---

## Supported Controllers

### ✅ Fully Supported
- **PlayStation**: DualShock 4 (PS4), DualSense (PS5)
- **Xbox**: Xbox One, Xbox Series X|S Controllers
- **Nintendo Switch**: Pro Controller, Joy-Cons (when paired)
- **Generic**: Any DirectInput or XInput compatible gamepad

---

## Controller Button Mappings

### Unity Generic Gamepad Path Reference

Unity's Input System uses **generic paths** that automatically map to the correct buttons across all controller types:

| Generic Path                | Xbox                | PlayStation      | Nintendo Switch     |
| --------------------------- | ------------------- | ---------------- | ------------------- |
| `<Gamepad>/buttonSouth`     | A                   | Cross (✕)        | B                   |
| `<Gamepad>/buttonEast`      | B                   | Circle (○)       | A                   |
| `<Gamepad>/buttonWest`      | X                   | Square (□)       | Y                   |
| `<Gamepad>/buttonNorth`     | Y                   | Triangle (△)     | X                   |
| `<Gamepad>/leftShoulder`    | LB                  | L1               | L                   |
| `<Gamepad>/rightShoulder`   | RB                  | R1               | R                   |
| `<Gamepad>/leftTrigger`     | LT                  | L2               | ZL                  |
| `<Gamepad>/rightTrigger`    | RT                  | R2               | ZR                  |
| `<Gamepad>/leftStick`       | Left Stick          | Left Stick (L3)  | Left Stick          |
| `<Gamepad>/rightStick`      | Right Stick         | Right Stick (R3) | Right Stick         |
| `<Gamepad>/leftStickPress`  | Left Stick (click)  | L3 (press)       | Left Stick (click)  |
| `<Gamepad>/rightStickPress` | Right Stick (click) | R3 (press)       | Right Stick (click) |
| `<Gamepad>/dpad`            | D-Pad               | D-Pad            | D-Pad               |
| `<Gamepad>/start`           | Menu                | Options          | + (Plus)            |
| `<Gamepad>/select`          | View                | Share/Create     | - (Minus)           |

---

## Game Controls

### Movement & Camera

| Action           | Keyboard/Mouse    | Xbox               | PlayStation   | Switch        |
| ---------------- | ----------------- | ------------------ | ------------- | ------------- |
| **Move**         | WASD              | Left Stick         | Left Stick    | Left Stick    |
| **Camera Orbit** | Left Mouse Button | Right Stick        | Right Stick   | Right Stick   |
| **Camera Zoom**  | Mouse Scroll      | D-Pad Up/Down      | D-Pad Up/Down | D-Pad Up/Down |
| **Jump**         | Space             | A                  | Cross (✕)     | B             |
| **Sprint**       | Left Shift        | LT (Left Trigger)  | L2            | ZL            |
| **Crouch**       | Left Control      | LB (Left Shoulder) | L1            | L             |

### Input System Implementation Notes

**Movement:**
- Left Stick provides 2D vector input (-1 to 1 on both axes)
- Dead zone automatically handled by Unity Input System

**Camera Control:**
- Right Stick for orbital camera movement (replaces LMB/RMB on mouse)
- Sensitivity configurable in `CameraController`

**Triggers vs Buttons:**
- Triggers (`leftTrigger`, `rightTrigger`) return analog values (0.0 to 1.0)
- We use triggers as binary buttons (pressed/not pressed) for simplicity
- Can be upgraded to analog speed control in the future

**D-Pad for Zoom:**
- D-Pad Up (`<Gamepad>/dpad/up`) = Zoom In
- D-Pad Down (`<Gamepad>/dpad/down`) = Zoom Out
- Unity represents D-Pad as 4 separate buttons

---

## Platform-Specific Controller Notes

### PlayStation Controllers (DualShock 4 / DualSense)

**Connection:**
- **Windows**: USB (plug and play) or Bluetooth (paired via Windows Settings)
- **macOS**: USB or Bluetooth
- **Features**: Full button support, rumble (limited over Bluetooth on some platforms)

**Unity Path Examples:**
```
<DualShockGamepad>/circle      // Circle button (PS4)
<DualSenseGamepadHID>/square   // Square button (PS5)
<Gamepad>/leftStick            // Generic left stick (works for both)
```

### Xbox Controllers

**Connection:**
- **Windows**: Xbox Wireless Adapter, Bluetooth, or USB
- **Features**: Native Windows support, full rumble

**Unity Path Examples:**
```
<XInputController>/buttonSouth  // A button
<Gamepad>/rightTrigger          // RT trigger
```

### Nintendo Switch Controllers

**Connection:**
- **Windows/macOS**: Bluetooth (pair via OS settings)
- **Pro Controller**: Best support
- **Joy-Cons**: Work when paired together as one controller

**Unity Path Examples:**
```
<SwitchProControllerHID>/buttonSouth  // B button on Switch
<Gamepad>/leftShoulder                // L button
```

**Important:** Switch button layout is reversed from Xbox:
- Switch **A** = Xbox **B** (buttonEast)
- Switch **B** = Xbox **A** (buttonSouth)

---

## Input System Architecture

### Programmatic Bindings

The game uses **programmatic action creation** instead of Input Action Assets for flexibility:

**Advantages:**
- No asset files to manage
- Easy to add/modify bindings in code
- Same script works across all controllers
- Automatic fallback to keyboard

**How It Works:**
```csharp
// Example from PlayerController.cs
jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
jumpAction.AddBinding("<Gamepad>/buttonSouth"); // Cross/A/B button
jumpAction.Enable();
```

### Extensibility

**To Add New Actions:**
1. Create new `InputAction` in `SetupInput()`
2. Add keyboard binding as primary
3. Add controller bindings via `AddBinding()`
4. Enable the action
5. Read value in `Update()` or appropriate method

**Example - Adding a "Dodge" action:**
```csharp
dodgeAction = new InputAction("Dodge", binding: "<Keyboard>/leftAlt");
dodgeAction.AddBinding("<Gamepad>/buttonEast"); // B/Circle/A
dodgeAction.Enable();

// In Update or HandleDodge:
if (dodgeAction.WasPressedThisFrame()) {
    // Dodge logic
}
```

---

## Troubleshooting

### Controller Not Detected
1. **Check Connection**: Ensure controller is connected (wired or Bluetooth paired)
2. **Windows**: Go to `Settings > Devices > Bluetooth` (for wireless)
3. **Unity Input Debugger**: `Window > Analysis > Input Debugger` to see if controller is recognized
4. **Driver Issues**: Some controllers need specific drivers (especially PS5 DualSense)

### Wrong Button Mappings
- Unity's generic paths (`<Gamepad>/buttonSouth`, etc.) should auto-map correctly
- If using specific controller paths (e.g., `<DualShock>`), verify controller type
- Test with Input Debugger to see actual button presses

### Stick Drift / Dead Zone
- Adjust dead zone in Unity Input System settings
- Can be set per-action or globally
- Default dead zone is usually 0.125

### Camera Movement Too Sensitive
- Adjust `cameraSensitivity` in `CameraController` Inspector
- Controller stick sensitivity separate from mouse

---

## Future Enhancements

**Potential Additions:**
- Rumble/Haptic feedback on impact
- Analog speed control based on trigger pressure
- Custom button remapping UI
- Split-screen local multiplayer (controller per player)
- Adaptive triggers on DualSense (PS5)

---

**Last Updated**: 2025-11-26  
**Input System Version**: Unity Input System (com.unity.inputsystem)
