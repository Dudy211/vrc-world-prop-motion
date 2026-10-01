# VRC World Prop Motion (VRC 世界物体运动插件)

[![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/Language-中文-red.svg)](README_CN.md)
[![日本語](https://img.shields.io/badge/Language-日本語-green.svg)](README_JP.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)

**VRC World Prop Motion** 是一款功能强大的 **UdonSharp** 插件，专为 **VRChat 世界**创作者打造。无需编写代码，即可实现复杂的物体移动、旋转、物理交互、拖拽操作和网络同步。

非常适合制作电梯、滑动门、精密旋钮、物理轨道、多米诺连锁和靠近触发机关。

## ✨ 核心特性

### 🔄 双角色架构 (Sender / Receiver)
*   **Receiver (接收端)**: 挂载在**被移动的物体**上，负责执行位移和旋转。
*   **Sender (发送端)**: 挂载在**按钮或手柄**上，用于触发 Receiver。
*   支持 **Interact (点击)**、**Drag (拖拽)** 和 **Proximity (靠近触发)** 三种触发模式。

### 📐 多样化运动
*   **移动 (Move)**: 直线或曲线路径（AnimationCurve 控制拱形高度）。
*   **旋转 (Rotate)**:
    *   **球面旋转 (Spherical)**: 在两个朝向之间平滑插值。
    *   **轴旋转 (Axis)**: 绕自定义轴旋转（手动坐标、物体对齐、欧拉角）。支持多圈转动（如 3600° 旋钮）。
*   **路径点 (Path Points)**: 按顺序经过多个 Transform 路径点。

### 🎢 曲线与物理
*   **曲线运动**: AnimationCurve 定义拱桥高度和速度缓动。
*   **重力与阻力**: 模拟真实的轨道物理（重力驱动下坡、阻力减速）。
*   **速度模式**: 固定速度或曲线驱动的可变速度。

### 🔗 连锁与事件
*   **多米诺连锁**: 到达终点/返回起点时触发其他 Drag 组件（`nextOnReach` / `nextOnReturn`）。
*   **路径点事件**: 经过每个路径点时调用 UdonBehaviour 的 `OnPathPoint()`。
*   **回调事件**: `OnReachDestination()` 和 `OnReachStart()` 自定义事件。

### 🌐 网络同步
*   `UdonBehaviourSyncMode.Continuous`，可配置同步帧间隔和最小变化阈值。
*   非主人端平滑插值显示。
*   交互时自动转移所有权。

### 🎨 编辑器增强
*   **三语界面**: 英文、日文、中文。
*   **Scene 视图工具**:
    *   轴起点/终点拖拽手柄。
    *   曲线原点拖拽（黄色球体）。
    *   世界轴对齐按钮（X/Y/Z）。
    *   起点到终点之间的幽灵网格预览。
    *   连锁关系预览（上下游 Drag 可视化）。
*   **自动初始化**: 导入时自动生成 `Drag.asset`。

## 📦 安装

1.  确保已通过 **VCC** 安装 **UdonSharp**（随 Worlds SDK 附带）。
2.  从 [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases) 下载 `vrc-world-prop-motion.unitypackage`。
3.  导入 Unity。若 `Drag.asset` 未自动生成，使用 **`Tools > VRC Prop Motion > Setup Drag Program Asset`**。

## 🚀 快速开始

### 接收端（被移动物体）
1.  将 `Drag.cs` 挂载到要移动的物体上。
2.  **Role** → `Receiver`，**Motion Mode** → `Move`。
3.  **Target** 设为自身（或留空默认）。
4.  配置 **Destination Mode**（偏移向量 / 目标变换 / 路径点）。
5.  通过 Interact、Drag 或 Proximity 触发。

### 发送端 + 接收端（可拖拽门）
1.  **Receiver**: 挂在门上，设置目的地。
2.  **Sender**: 挂在门把手上。**Role** → `Sender`，指定 **Receiver**。
3.  **Trigger Move Mode**: `Rail`（沿轨道滑动）或 `SyncTarget`（跟随门）。
4.  游戏中抓取把手即可推拉门。

### 靠近触发
1.  在机关物体上挂 Receiver。**Interaction Mode** → `Proximity`。
2.  在同一物体上添加 **Collider**（勾选 Is Trigger）。
3.  玩家进入碰撞体范围即自动启动。

### 轴旋转（旋钮/阀门）
1.  在旋钮上挂 Receiver。**Motion Mode** → `Rotate`，**Rotate Mode** → `Axis`。
2.  **Axis Source** → `Object Align`（对齐旋钮本地 Y 轴）。
3.  **Axis Angle** → `360`（或 `720`、`3600` 多圈）。
4.  添加 Sender（把手）并配置 Drag 交互。

## ⚠️ 故障排除

| 问题 | 解决方案 |
|------|----------|
| `Program Source is None` | 使用 `Tools > VRC Prop Motion > Setup Drag Program Asset` |
| `Drag.cs referenced by 2 UdonSharpProgramAssets` | 删除 `Drag.asset` 让 UdonSharp 用已有引用，或重命名 `Drag.cs` |
| 拖拽无反应 | Trigger 需要 `VRC_Pickup` + `Collider` + `Rigidbody` |
| 场景轴手柄难拖 | 拖 XYZ 彩色箭头而非中心；或先在 Inspector 输入小偏移 |
| 修改 Drag.cs 字段后 Inspector 异常 | 删除 `Drag.asset` + `.meta`，通过 Tools 菜单重新生成 |

## 📜 许可证
MIT License — 详见 [LICENSE](LICENSE)。
