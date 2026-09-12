# VRC World Prop Motion

[![Latest Release](https://img.shields.io/github/v/release/Dudy211/vrc-world-prop-motion?label=latest%20release&color=blue)](https://github.com/Dudy211/vrc-world-prop-motion/releases)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)
[![Languages](https://img.shields.io/badge/languages-EN%20%7C%20%E4%B8%AD%E6%96%87%20%7C%20%E6%97%A5%E6%9C%AC%E8%AA%9E-blue)](./README_CN.md)

A lightweight **UdonSharp** plugin for **VRChat Worlds** that lets you animate
**Props (world objects)** with precise motion control.

Define **movement and rotation sequences** using curves, physics integration,
and drag interaction — and keep everything **synced across the network**.

> Also available in: [中文](./README_CN.md) · [日本語](./README_JP.md)

---

## ✨ Features

- **Transform Control** — Move and rotate objects along configurable paths.
- **Curves & Physics** — Drive motion with `AnimationCurve` and gravity.
- **Interaction** — Trigger animations via `Interact` or **Drag** components.
- **Network Sync** — Smooth synchronization across all clients.
- **Localization** — Inspector UI supports **English · 中文 · 日本語**.

---

## 📦 Installation

### Option A: Unitypackage (Recommended)

1. Download the latest `vrc-world-prop-motion-x.y.z.unitypackage` from the
   [**Releases page**](https://github.com/Dudy211/vrc-world-prop-motion/releases).
2. **Double-click** the file, or go to `Assets > Import Package > Custom Package…`
   in Unity, then import it into your project.
   > ⚠️ **Important:** make sure every `.cs` file is paired with its `.meta`
   > file in the import list — otherwise script references will break.
3. Ensure **UdonSharp** is installed via **VCC**
   (it ships with the **VRChat Worlds SDK**).
4. The plugin lives in `Assets/VRCPropMotion/`.

### Option B: Clone / Download (For Developers)

1. Make sure **UdonSharp** is installed in your project.
2. Clone this repo or download the ZIP, then copy the `VRCPropMotion/` folder
   into your `Assets/` directory.

---

## 🚀 Quick Start

1. Import the package (Option A above).
2. In your scene, select the **Prop** object you want to animate.
3. `Add Component > Drag` (the UdonSharp behaviour).
4. Configure **curves, physics, and interaction** in the Inspector.
5. Enter **Play Mode** to test the motion and network sync.

---

## 🔧 Requirements

| Dependency | Version |
|---|---|
| Unity | `2019.4.31f1` (VRChat World Starter Template) |
| VRChat Worlds SDK | latest (via VCC) |
| UdonSharp | latest (ships with Worlds SDK) |

---

## 🐛 Troubleshooting

### `Program Source is None` / asset not auto-generated
The plugin's **Editor scripts** automatically create the required
`UdonSharpProgramAsset` (`.asset`) on import and bind it to `Drag.cs`.
- Check the **Console** for the `[VRCPropMotion] 已生成 / Generated Drag.asset`
  log message.
- If it did **not** run, use the manual fallback menu:
  `Tools > VRC Prop Motion > Setup Drag Program Asset`.

### `Script Drag.cs is referenced by 2 UdonSharpProgramAssets`
This means **another asset in your project** (e.g. a downloaded prefab like
Ohmiwa's `SlantedCeilingRoom`) **also references a `Drag.cs`**.
UdonSharp requires **one script = one program asset**.

**Fix (pick one):**
1. **Delete** `Assets/VRCPropMotion/Drag.asset`, let the other asset handle it.
2. **Rename** the script/class to a unique name (e.g. `VRCPropMotionDrag`)
   to avoid collisions — **recommended for distribution**.
3. Re-open Unity and let the Editor scripts re-link everything.

### Import errors / wrong meta files
- Never include **UdonSharp / VRCSDK** folders in your own `.unitypackage`
  (users install them via VCC).
- Always **keep `.meta` files** so GUID references survive the import.

See [CHANGELOG.md](./CHANGELOG.md) for version history.

---

## 🤝 Contributing

Issues and Pull Requests are welcome!
If you find a bug or have a feature idea, please open an Issue.

This is an **early beta**, so help from experienced VRChat / UdonSharp
developers is greatly appreciated 🙏

---

## 📝 License

[MIT](./LICENSE) © 2026 [Dudy211](https://github.com/Dudy211)
