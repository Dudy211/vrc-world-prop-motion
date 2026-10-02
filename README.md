# VRC World Prop Motion

[![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/Language-中文-red.svg)](README_CN.md)
[![日本語](https://img.shields.io/badge/Language-日本語-green.svg)](README_JP.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)
[![Latest Release](https://img.shields.io/github/v/release/Dudy211/vrc-world-prop-motion)](https://github.com/Dudy211/vrc-world-prop-motion/releases)

**VRC World Prop Motion** is a powerful **UdonSharp** plugin for **VRChat World** creators. Control props with precision—movement, rotation, physics, drag interaction, hover feedback, and network sync—no coding required.

Perfect for elevators, sliding doors, precision knobs, physical tracks, domino chains, proximity-activated mechanisms, and interactive exhibits.

## ✨ Key Features

### 🔄 Sender / Receiver Architecture
*   **Receiver**: Attached to the object being moved. Executes transformations.
*   **Sender**: Attached to buttons/handles. Triggers the Receiver.
*   Three trigger modes: **Interact** (click), **Drag** (manual grab), **Passive** (player enters zone).

### 🚶 Passive / Proximity Trigger
*   **Passive mode**: Automatically wraps a `BoxCollider` trigger zone around the Receiver.
*   Movement starts when a player **enters** the zone (teleporting/spawning inside does NOT count).
*   Configurable zone center and size in local space.

### 🖱️ Hover Feedback System
*   **Native**: Uses VRChat's built-in interaction prompt (requires Interactive layer).
*   **Custom**: Full control over your own visuals.
    *   **Hover Visual**: Show/hide a GameObject when the local player aims at the trigger.
    *   **Hover Highlight**: Tint target renderers via `MaterialPropertyBlock` (per-client, no material duplication).
    *   **Hover Text**: World-space text prompt auto-created by the editor (positioned above the object).
    *   Configurable distance, blink speed, color, font, and offset.

### 📐 Advanced Motion
*   **Move**: Linear or curve paths (AnimationCurve-controlled arches).
*   **Rotate**:
    *   **Spherical**: Slerp between two orientations.
    *   **Axis**: Rotate around a custom axis (Manual, Object Align, or Euler angle). Supports multi-turn (e.g., 3600° knobs).
*   **Path Points**: Sequence through multiple Transform waypoints. Optional closed-loop.

### 🎢 Curves & Physics
*   **Curve Movement**: AnimationCurve defines arch height and speed tweening.
*   **Gravity & Drag**: Simulate realistic physics on rails (gravity accelerates downhill, drag dampens velocity).
*   **Speed Modes**: Fixed speed or curve-driven variable speed.
*   **SnapBack**: Handle auto-returns to start at configurable speed after release.

### 🔗 Chain & Events
*   **Domino Chains**: Trigger other Drags when reaching end/start (`nextOnReach`, `nextOnReturn`).
*   **Path Point Events**: Call `OnPathPoint()` on UdonBehaviours at each waypoint.
*   **Callbacks**: `OnReachDestination()`, `OnReachStart()`, `OnGrabbed()`, `OnReleased()`.
*   **Chain Events Toggle**: Master switch to disable all events/chains at once.

### 🔊 Sound Effects
*   Optional AudioSource integration.
*   Play sounds for: reaching destination, returning to start, grabbing, and releasing.
*   Sounds play locally only.

### 🌐 Networking
*   `UdonBehaviourSyncMode.Continuous` with configurable `syncEveryNFrames` and `syncMinDelta`.
*   Smooth interpolation for non-owners on deserialization.
*   Auto ownership transfer on interaction.
*   **LockWhileHeld**: Prevent other players from triggering while someone holds the handle.

### 🎨 Editor Enhancements
*   **Trilingual UI**: English, 日本語, 中文.
*   **Scene View Tools**:
    *   Axis start/end/midpoint handles (drag in Scene).
    *   Curve origin handle (yellow sphere, free or per-axis drag).
    *   World axis alignment buttons (X/Y/Z).
    *   Ghost mesh/wireframe slices between start and end.
    *   Chain relation preview (upstream/downstream Drags).
*   **Scrub Preview**: Slide through progress (0–1) to preview poses. Set/apply pose base.
*   **Auto-Initialization**: `Drag.asset` generated automatically on import.
*   **Built-in Help**: Quick start, asset info, sync explanation, troubleshooting.

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
5.  Trigger via Interact, Drag, or Passive.

### Sender + Receiver (Draggable Door)
1.  **Receiver**: On the door. Set destination.
2.  **Sender**: On the door handle. Set **Role** → `Sender`, assign **Receiver**.
3.  **Trigger Move Mode**: `Rail` (slides along track) or `SyncTarget` (follows door).
4.  Grab handle in-game to slide door.

### Passive Trigger (Proximity Zone)
1.  Receiver on the mechanism. **Interaction Mode** → `Passive`.
2.  Configure **Trigger Zone Center** and **Size** (auto-wraps a BoxCollider).
3.  Player entering the zone starts movement automatically.

### Hover Feedback
1.  Set **Hover Mode** → `Custom`.
2.  Assign **Hover Visual** (GameObject shown when aimed at).
3.  Enable **Hover Highlight** for renderer tinting, or **Hover Text** for world-space prompts.

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
| Passive mode not triggering | Ensure the auto-generated BoxCollider is enabled and large enough |
| After editing Drag.cs fields, Inspector shows wrong data | Delete `Drag.asset` + `.meta`, regenerate via Tools menu |

## 📜 License
MIT License — see [LICENSE](LICENSE).
