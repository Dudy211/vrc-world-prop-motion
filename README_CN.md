# VRC World Prop Motion（中文说明）

[![最新版本](https://img.shields.io/github/v/release/Dudy211/vrc-world-prop-motion?label=%E6%9C%80%E6%96%B0%E7%89%88%E6%9C%AC&color=blue)](https://github.com/Dudy211/vrc-world-prop-motion/releases)
[![许可证: MIT](https://img.shields.io/badge/%E8%AE%B8%E5%8F%AF%E8%AF%81-MIT-green.svg)](./LICENSE)
[![English](https://img.shields.io/badge/English-README-blue)](./README.md) · [日本語](./README_JP.md)

一个轻量级的 **UdonSharp** 插件，用于 **VRChat 世界**，让你对
**场景道具（Props）** 进行精确的运动控制。

通过**曲线、物理和拖拽交互**定义物体的移动与旋转序列，并**在网络间同步**。

---

## ✨ 功能特性

- **变换控制** — 沿可配置路径移动、旋转物体。
- **曲线与物理** — 用 `AnimationCurve` 和重力驱动运动。
- **交互** — 通过 `Interact` 或 **Drag** 组件触发动画。
- **网络同步** — 在所有客户端间平滑同步。
- **多语言** — 检视面板支持 **English · 中文 · 日本語**。

---

## 📦 安装方式

### 方式 A：Unitypackage（推荐）

1. 从 [**Releases 页面**](https://github.com/Dudy211/vrc-world-prop-motion/releases)
   下载最新的 `vrc-world-prop-motion-x.y.z.unitypackage`。
2. 在 Unity 里**双击该文件**，或 `Assets > Import Package > Custom Package…` 导入。
   > ⚠️ **注意：** 导入列表里每个 `.cs` 都必须有配对的 `.meta`，否则引用会断。
3. 确保已通过 **VCC** 安装 **UdonSharp**
   （它随 **VRChat Worlds SDK** 一起提供）。
4. 插件位于 `Assets/VRCPropMotion/`。

### 方式 B：克隆 / 下载（开发者）

1. 确保项目已装 **UdonSharp**。
2. 克隆仓库或下载 ZIP，把 `VRCPropMotion/` 文件夹复制到 `Assets/` 下。

---

## 🚀 快速上手

1. 按方式 A 导入包。
2. 在场景里选中要动画化的 **道具物体**。
3. `Add Component > Drag`（即 UdonSharp 行为）。
4. 在检视面板配置**曲线、物理、交互**。
5. 进入 **Play Mode** 测试运动与网络同步。

---

## 🔧 环境要求

| 依赖 | 版本 |
|---|---|
| Unity | `2019.4.31f1`（VRChat World 模板） |
| VRChat Worlds SDK | 最新（VCC 安装） |
| UdonSharp | 最新（随 Worlds SDK 提供） |

---

## 🐛 故障排查

### `Program Source is None` / 未自动生成 asset
插件的 **Editor 脚本**会在导入时自动创建所需的
`UdonSharpProgramAsset`（`.asset`）并绑定到 `Drag.cs`。
- 查看 **Console** 是否出现 `[VRCPropMotion] 已生成 Drag.asset` 日志。
- 若未执行，可用菜单手动兜底：
  `Tools > VRC Prop Motion > Setup Drag Program Asset`。

### `Script Drag.cs is referenced by 2 UdonSharpProgramAssets`
说明工程里**另一个资源**（如下载的 Ohmiwa `SlantedCeilingRoom` prefab）
**也引用了某个 `Drag.cs`**。UdonSharp 规定**一个脚本只能对应一个 program asset**。

**解决方法（任选其一）：**
1. **删除** `Assets/VRCPropMotion/Drag.asset`，让另一个 asset 接管。
2. **重命名**脚本/类为唯一名（如 `VRCPropMotionDrag`）避免冲突 ——
   **发布时推荐此方案**。
3. 重新打开 Unity，让 Editor 脚本重新关联。

### 导入报错 / meta 错乱
- 自己的 `.unitypackage` **绝不要包含 UdonSharp / VRCSDK** 文件夹（让用户走 VCC）。
- **务必保留 `.meta` 文件**，这样 GUID 引用才能正确保留。

版本历史见 [CHANGELOG.md](./CHANGELOG.md)。

---

## 🤝 贡献

欢迎提 Issue 和 Pull Request！
发现 Bug 或有功能建议，请开 Issue。

本项目处于**早期 Beta 阶段**，非常感谢有经验的 VRChat / UdonSharp
开发者提供帮助 🙏

---

## 📝 许可证

[MIT](./LICENSE) © 2026 [Dudy211](https://github.com/Dudy211)
