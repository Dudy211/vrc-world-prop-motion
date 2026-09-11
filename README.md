# VRC Prop Motion
### Multi-language / 多言語対応
[**English**](README.md) | [**中文**](README_CN.md) | [**日本語**](README_JP.md)

---

A lightweight UdonSharp plugin for VRChat Worlds to animate **Props** (World Objects) with precise **Motion** control.

Define movement and rotation sequences with curves, physics integration, and drag interaction—all synced across the network.

![Preview](https://img.shields.io/badge/Status-Beta-blue) ![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features
* **Transform Animation**: Control Position and Rotation over time.
* **Curve Editor**: Utilize Unity's Animation Curves for smooth easing.
* **Physics Interaction**: Optional Rigidbody integration.
* **Drag Trigger**: Start animations via VRChat's `Drag` component.
* **Network Sync**: Synchronize motion states across all clients using UdonSynced.
* **Editor Localization**: Inspector UI supports English, 中文, and 日本語.

## 📦 Installation
1. Ensure **UdonSharp** is installed in your project.
2. Download this repository.
3. Copy the `VRCPropMotion` folder into your Unity project's `Assets` directory.
4. *(Optional)* Place the `Editor` folder contents in an `Editor` assembly definition if you use AsmDef.

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
* **Target**: The GameObject to animate.
* **Duration**: Total time for the motion.
* **Start On Load**: Auto-play when the world loads.

### Motion
* **Move From/To**: Start and end positions.
* **Use Local Space**: Toggle between local and world coordinates.

### Rotation
* **Rotate From/To**: Start and end rotations.
* **Rotation Mode**: Set to `None`, `Linear`, or `Curve`.

### Networking
* **Sync Motion**: Enable state synchronization across clients.

## 📜 License
This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
