# VRC World Prop Motion (VRC 世界物体运动插件)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)

**VRC World Prop Motion** 是一款功能强大的 **UdonSharp** 插件，专为 **VRChat 世界**创作者打造。
无需编写代码，只需在 Inspector 面板中配置，即可实现复杂的物体移动、旋转、物理交互与网络同步。

非常适合制作电梯、滑动门、精密旋钮、物理轨道等交互逻辑。

## ✨ 核心特性

### 🔄 双角色架构 (Sender / Receiver)
*   **Receiver (接收者)**: 挂载在**被移动的物体**上，负责执行实际的位移和旋转。
*   **Sender (发送者)**: 挂载在**按钮或手柄**上，用于触发 Receiver。
*   支持 **Interact (点击)** 和 **Drag (拖拽)** 两种触发模式。

### 📐 多样化的运动模式
*   **移动 (Move)**: 沿直线或曲线移动物体。
*   **旋转 (Rotate)**:
    *   **球面旋转 (Spherical)**: 在两个朝向之间进行平滑插值。
    *   **轴旋转 (Axis)**: 绕自定义轴（手动定义、物体对齐或欧拉角）旋转，支持多圈转动（如 3600° 旋钮）。

### 🎢 路径与曲线
*   **路径点 (Path Points)**: 按顺序经过一系列 Transform 位置。
*   **曲线运动 (AnimationCurve)**: 定义移动过程中的拱形高度和速度变化（缓动效果）。
*   **自动原点**: 自动计算路径中点作为曲线基准，简化配置。

### 🛠️ 物理与交互
*   **轨道模式 (Rail)**: 拖拽时限制物体沿预设轨道滑动，适合推拉门。
*   **同步目标 (SyncTarget)**: 拖拽时手柄紧贴物体，适合精密操作（如拧阀门）。
*   **重力与阻力**: 模拟真实的物理滑动效果。
*   **循环与往返 (Loop / PingPong)**: 灵活的路径循环模式。

### 🌐 网络同步
*   基于 `UdonBehaviourSyncMode.Continuous`，确保所有玩家看到的动画状态一致。
*   自动处理所有权转移。

### 🎨 编辑器增强
*   **三语支持**: Inspector 界面支持 **English / 日本語 / 中文**。
*   **Scene 视图辅助**:
    *   实时预览目标位置和路径。
    *   可视化旋转轴和移动轨道。
    *   支持在 Scene 视图中直接拖拽调整轴起点、终点、曲线原点和偏移向量。
*   **自动初始化**: 导入包后自动生成 `Drag.asset`，解决 UdonSharp 引用丢失的痛点。

## 📦 安装

1.  确保你的项目已通过 **VCC (VRChat Creator Companion)** 安装了 `UdonSharp` (随 Worlds SDK 安装)。
2.  前往 [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases) 页面下载最新的 `vrc-world-prop-motion.unitypackage`。
3.  导入 Unity 项目。
4.  如果 `Drag.asset` 未自动生成，请点击菜单 **`Tools > VRC Prop Motion > Setup Drag Program Asset`**。

## 🚀 快速开始

### 场景物体移动 (Receiver)
1.  将 `Drag.cs` 挂载到你想移动的物体上。
2.  **Role** 设为 `Receiver`。
3.  **Motion Mode** 选择 `Move`。
4.  调整 **Destination Mode** (如 Offset Vector) 和 **Offset** 值。
5.  点击 Play，或使用 Interact/Drag 触发。

### 拖拽门/抽屉 (Sender + Receiver)
1.  **Receiver**: 挂在门上，设置好终点位置。
2.  **Sender**: 挂在门把手上。
    *   **Role**: `Sender`
    *   **Receiver**: 拖入刚才的门物体。
    *   **Interaction Mode**: `Drag`
    *   **Trigger Move Mode**: `Rail` (沿轨道滑动)。
3.  进入游戏，抓取把手即可拖动门。

## ⚠️ 故障排除

### 1. `Program Source is None` 或 编译错误
*   确保 `Assets/VRCPropMotion/Drag.asset` 存在。
*   如果不存在，请使用菜单 `Tools > VRC Prop Motion > Setup Drag Program Asset` 生成。

### 2. `Drag.cs is referenced by 2 UdonSharpProgramAssets`
*   **原因**: 你的项目中存在另一个插件（如 Ohmiwa 的示例）也使用了名为 `Drag.cs` 的脚本。
*   **解决**:
    *   删除 `Assets/VRCPropMotion/Drag.asset`，让 UdonSharp 使用已有的引用。
    *   或者，重命名本插件的 `Drag.cs` 为 `VRCPropMotionDrag.cs` (需同步修改 Editor 脚本)。

## 📜 许可证
本项目基于 **MIT License** 开源。详见 [LICENSE](LICENSE) 文件。

---
💡 **提示**: 如果你觉得这个工具好用，欢迎 Star 支持！如有 Bug 或建议，请在 Issues 中提出。
