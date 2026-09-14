# VRC World Prop Motion

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)

**VRC World Prop Motion** is a robust **UdonSharp** plugin designed for **VRChat World** creators. 
It allows you to animate props (objects) with precise control over movement, rotation, physics, and networking—no coding required.

Perfect for elevators, sliding doors, intricate knobs, and physical tracks.

## ✨ Key Features

### 🔄 Sender / Receiver Architecture
*   **Receiver**: Attached to the object being moved. Executes transformations.
*   **Sender**: Attached to buttons or handles. Triggers the Receiver.
*   Supports **Interact** (click) and **Drag** (manual control) triggers.

### 📐 Advanced Motion Modes
*   **Move**: Linear movement between points.
*   **Rotate**:
    *   **Spherical**: Smoothly interpolate between two rotations.
    *   **Axis**: Rotate around a custom axis (Manual, Object Align, or Euler). Supports multi-turn rotations (e.g., 3600° knobs).

### 🎢 Paths & Curves
*   **Path Points**: Move through a sequence of Transforms.
*   **Animation Curves**: Define height arcs and speed tweening (Easing).
*   **Auto-Origin**: Automatically calculates path mid-points for curves.

### 🛠️ Physics & Interaction
*   **Rail Mode**: Constrain dragging to a specific track.
*   **Sync Target**: Hand syncs to the object for precise control.
*   **Gravity & Drag**: Simulate realistic sliding physics.
*   **Loop & PingPong**: Flexible path looping.

### 🌐 Networking
*   Stable synchronization using `UdonBehaviourSyncMode.Continuous`.
*   Automatic ownership transfer handling.

### 🎨 Editor Enhancements
*   **Trilingual UI**: Inspector supports **English, 日本語, and 中文**.
*   **Scene View Gizmos**:
    *   Visualize paths, destinations, and rotation axes.
    *   Drag handles for adjusting vectors directly in the Scene view.
*   **Auto-Initialization**: Automatically generates `Drag.asset` to prevent UdonSharp reference errors.

## 📦 Installation

1.  Ensure **UdonSharp** is installed via **VCC** (included with the Worlds SDK).
2.  Download the latest `vrc-world-prop-motion.unitypackage` from the [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases) page.
3.  Import it into your Unity project.
4.  If `Drag.asset` fails to generate, use the menu: **`Tools > VRC Prop Motion > Setup Drag Program Asset`**.

## 🚀 Quick Start

### Moving a Prop (Receiver)
1.  Attach `Drag.cs` to your prop.
2.  Set **Role** to `Receiver`.
3.  Set **Motion Mode** to `Move`.
4.  Configure **Destination Mode** (e.g., Offset Vector) and adjust the **Offset**.
5.  Enter Play mode or trigger via Interact/Drag.

### Creating a Draggable Door (Sender + Receiver)
1.  **Receiver**: Attach to the door. Set the destination position.
2.  **Sender**: Attach to the door handle.
    *   **Role**: `Sender`
    *   **Receiver**: Assign the door object.
    *   **Interaction Mode**: `Drag`
    *   **Trigger Move Mode**: `Rail`.
3.  Grab the handle in-game to slide the door.

## ⚠️ Troubleshooting

### 1. `Program Source is None` Error
*   Ensure `Assets/VRCPropMotion/Drag.asset` exists.
*   If missing, use the menu `Tools > VRC Prop Motion > Setup Drag Program Asset`.

### 2. `Drag.cs is referenced by 2 UdonSharpProgramAssets`
*   **Cause**: A naming conflict with another plugin (e.g., Ohmiwa examples).
*   **Fix**: Delete `Assets/VRCPropMotion/Drag.asset` to let UdonSharp use the existing reference, or rename `Drag.cs` to `VRCPropMotionDrag.cs`.

## 📜 License
Licensed under the **MIT License**. See [LICENSE](LICENSE) for details.
