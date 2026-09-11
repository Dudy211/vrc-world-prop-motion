# VRC World Prop Motion

> **Latest Release:** [v1.0.0](https://github.com/Dudy211/vrc-world-prop-motion/releases/latest) · **License:** [MIT](LICENSE)

### Multi-language / 多言語対応
[**English**](README.md) | [**中文**](README_CN.md) | [**日本語**](README_JP.md)

---

A lightweight UdonSharp plugin for VRChat Worlds to animate **Props** (World Objects) with precise **Motion** control.

Define movement and rotation sequences with curves, physics integration, and drag interaction—all synced across the network.

## ✨ Features
- **Transform Animation**: Control Position and Rotation over time.
- **Curve Editor**: Utilize Unity's Animation Curves for smooth easing.
- **Physics Interaction**: Optional Rigidbody integration.
- **Drag Trigger**: Start animations via VRChat's `Drag` component.
- **Network Sync**: Synchronize motion states across all clients using UdonSynced.
- **Editor Localization**: Inspector UI supports English, 中文, and 日本語.

## 📦 Installation

### Option A: Unitypackage (Recommended)
1. Download the latest `vrc-world-prop-motion-x.y.z.unitypackage` from the [**Releases Page**](https://github.com/Dudy211/vrc-world-prop-motion/releases).
2. Import it into your Unity project:
   - Double-click the `.unitypackage` file, **or**
   - Go to `Assets > Import Package > Custom Package...` and select the file.
3. Make sure **UdonSharp** is installed and up to date.

### Option B: Clone / Download (For Developers)
1. Ensure **UdonSharp** is installed.
2. Clone this repository or download the ZIP.
3. Copy the plugin folder into your `Assets` directory (keep all `.meta` files).

## 🚀 Quick Start
1. Create an empty GameObject in your world.
2. Add the **`PropMotion`** component.
3. Assign the **Target** (the object to move/rotate).
4. Configure the **Motion Settings** (Duration, Curve, etc.).
5. **Trigger**:
   - Use a **Udon Event** (e.g., `Interact`, `OnTriggerEnter`) to call `StartMotion()`.
   - Or, add a **`DragTriggerRelay`** component to link it with a `Drag` component.

## 🛠️ Inspector Reference

### Core
- **Target**: The GameObject to animate.
- **Duration**: Total time for the motion.
- **Start On Load**: Auto-play when the world loads.

### Motion
- **Move From / To**: Start and end positions.
- **Use Local Space**: Toggle between local and world coordinates.

### Rotation
- **Rotate From / To**: Start and end rotations.
- **Rotation Mode**: `None`, `Linear`, or `Curve`.

### Networking
- **Sync Motion**: Enable state synchronization across clients.

## 📋 Requirements
- **UdonSharp** v1.0 or later
- **Unity** 2019.4.31f1 (VRChat World Starter Template)

## 🤝 Contributing
Issues and Pull Requests are welcome. Please open an issue first to discuss major changes.

## 📜 License
This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
