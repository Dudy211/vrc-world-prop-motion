# VRC World Prop Motion

[![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/Language-中文-red.svg)](README_CN.md)
[![日本語](https://img.shields.io/badge/Language-日本語-green.svg)](README_JP.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)

**VRC World Prop Motion** is a powerful **UdonSharp** plugin for **VRChat World** creators. Control props with precision—movement, rotation, physics, drag interaction, and network sync—no coding required.

Perfect for elevators, sliding doors, precision knobs, physical tracks, domino chains, and proximity-activated mechanisms.

## ✨ Key Features

### 🔄 Sender / Receiver Architecture
*   **Receiver**: Attached to the object being moved. Executes transformations.
*   **Sender**: Attached to buttons/handles. Triggers the Receiver.
*   Supports **Interact** (click), **Drag** (manual grab), and **Proximity** (player enters trigger) modes.

### 📐 Advanced Motion
*   **Move**: Linear or curve paths (AnimationCurve-controlled arches).
*   **Rotate**:
    *   **Spherical**: Slerp between two orientations.
    *   **Axis**: Rotate around a custom axis (Manual, Object Align, or Euler angle). Supports multi-turn (e.g., 3600° knobs).
*   **Path Points**: Sequence through multiple Transform waypoints.

### 🎢 Curves & Physics
*   **Curve Movement**: AnimationCurve defines arch height and speed tweening.
*   **Gravity & Drag**: Simulate realistic physics on rails (gravity accelerates downhill, drag dampens velocity).
*   **Speed Modes**: Fixed speed or curve-driven variable speed.

### 🔗 Chain & Events
*   **Domino Chains**: Trigger other Drags when reaching end/start (`nextOnReach`, `nextOnReturn`).
*   **Path Point Events**: Call `OnPathPoint()` on UdonBehaviours at each waypoint.
*   **Callbacks**: `OnReachDestination()` and `OnReachStart()` events.

### 🌐 Networking
*   `UdonBehaviourSyncMode.Continuous` with configurable sync interval and minimum delta threshold.
*   Smooth interpolation for non-owners.
*   Auto ownership transfer on interaction.

### 🎨 Editor Enhancements
*   **Trilingual UI**: English, 日本語, 中文.
*   **Scene View Tools**:
    *   Axis start/end handles (drag in Scene).
    *   Curve origin handle (yellow sphere).
    *   World axis alignment buttons (X/Y/Z).
    *   Ghost mesh previews between start and end.
    *   Chain relation preview (upstream/downstream Drags).
*   **Auto-Initialization**: `Drag.asset` generated automatically on import.

## 📦 Installation

1.  Ensure **UdonSharp** is installed via **VCC** (included with Worlds SDK).
2.  Download `vrc-world-prop-motion.unitypackage` from [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases).
3.  Import into Unity. If `Drag.asset` fails to generate, use **`Tools > VRC Prop Motion > Setup Drag Program Asset`**.

## 🚀 Quick Start

### Receiver (Moving Object)
1.  Attach `Drag.cs` to the object to move.
2.  Set **Role** → `Receiver`, **Motion Mode** → `Move`.
3.  Set **Target** to itself (or leave default).
4.  Configure **Destination Mode** (Offset Vector / Destination Transform / Path Points).
5.  Trigger via Interact, Drag, or Proximity.

### Sender + Receiver (Draggable Door)
1.  **Receiver**: On the door. Set destination.
2.  **Sender**: On the door handle. Set **Role** → `Sender`, assign **Receiver**.
3.  **Trigger Move Mode**: `Rail` (slides along track) or `SyncTarget` (follows door).
4.  Grab handle in-game to slide door.

### Proximity Trigger
1.  Receiver on the mechanism. **Interaction Mode** → `Proximity`.
2.  Add a **Collider** (Is Trigger = ON) on the **same GameObject**.
3.  Player entering the collider starts movement automatically.

### Axis Rotation (Knob/Dial)
1.  Receiver on the knob. **Motion Mode** → `Rotate`, **Rotate Mode** → `Axis`.
2.  **Axis Source**: `Object Align` (aligns to knob's local Y axis).
3.  **Axis Angle**: `360` (or `720`, `3600` for multi-turn).
4.  Add a Sender (handle) and configure Drag interaction.

## ⚠️ Troubleshooting

| Issue | Solution |
|-------|----------|
| `Program Source is None` | Use `Tools > VRC Prop Motion > Setup Drag Program Asset` |
| `Drag.cs referenced by 2 UdonSharpProgramAssets` | Delete `Drag.asset` to let UdonSharp use existing reference, or rename `Drag.cs` |
| Drag not responding | Trigger needs `VRC_Pickup` + `Collider` + `Rigidbody` |
| Scene axis handle hard to grab | Drag XYZ arrows instead of center; or type offset in Inspector first |
| After editing Drag.cs fields, Inspector shows wrong data | Delete `Drag.asset` + `.meta`, regenerate via Tools menu |

## 📜 License
MIT License — see [LICENSE](LICENSE).
