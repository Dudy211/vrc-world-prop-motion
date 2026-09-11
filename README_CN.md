# VRC World Prop Motion

> **最新版本：** [v1.0.0](https://github.com/Dudy211/vrc-world-prop-motion/releases/latest) · **协议：** [MIT](LICENSE)

### 多语言 / Multi-language
[**English**](README.md) | [**中文**](README_CN.md) | [**日本語**](README_JP.md)

---

一个轻量级的 UdonSharp 插件，用于在 VRChat 世界中控制**场景物体（Props）**的**运动（Motion）**。

你可以通过曲线定义移动和旋转序列，支持物理交互和拖拽触发，并且所有状态均支持**网络同步**。

## ✨ 功能特性
- **变换动画**：精确控制物体随时间的位移和旋转。
- **曲线编辑**：利用 Unity 的 Animation Curves 实现平滑缓动。
- **物理交互**：可选 Rigidbody 物理模拟支持。
- **拖拽触发**：通过 VRChat 的 `Drag` 组件启动动画。
- **网络同步**：使用 UdonSynced 在所有客户端间同步运动状态。
- **编辑器本地化**：Inspector 界面支持 英文、中文、日文。

## 📦 安装方法

### 方式 A：Unitypackage（推荐）
1. 从 [**Releases 页面**](https://github.com/Dudy211/vrc-world-prop-motion/releases) 下载最新的 `vrc-world-prop-motion-x.y.z.unitypackage`。
2. 导入到 Unity 项目中：
   - 双击 `.unitypackage` 文件，**或**
   - 菜单 `Assets > Import Package > Custom Package...`，选择该文件。
3. 确保 **UdonSharp** 已安装且为最新版本。

### 方式 B：Clone / 下载源码（面向开发者）
1. 确保项目中已安装 **UdonSharp**。
2. Clone 本仓库或下载 ZIP。
3. 将插件文件夹复制到 `Assets` 目录下（保留所有 `.meta` 文件）。

## 🚀 快速上手
1. 在场景中创建一个空物体。
2. 添加 **`PropMotion`** 组件。
3. 指定 **Target**（需要移动/旋转的物体）。
4. 配置 **Motion Settings**（持续时间、曲线等）。
5. **触发方式**：
   - 通过 **Udon 事件**（如 `Interact`、`OnTriggerEnter`）调用 `StartMotion()`。
   - 或者，添加 **`DragTriggerRelay`** 组件，将其与 `Drag` 组件关联。

## 🛠️ 参数说明

### 核心 (Core)
- **Target**：要执行动画的物体。
- **Duration**：动画总时长。
- **Start On Load**：世界加载时自动播放。

### 运动 (Motion)
- **Move From / To**：起始和结束位置。
- **Use Local Space**：切换局部坐标或世界坐标。

### 旋转 (Rotation)
- **Rotate From / To**：起始和结束旋转。
- **Rotation Mode**：`None`（不旋转）、`Linear`（线性）或 `Curve`（曲线）。

### 网络 (Networking)
- **Sync Motion**：开启后，运动状态将在所有客户端同步。

## 📋 环境要求
- **UdonSharp** v1.0 及以上
- **Unity** 2019.4.31f1（VRChat World Starter Template）

## 🤝 贡献
欢迎提交 Issue 和 Pull Request。较大的改动请先开 Issue 讨论。

## 📜 许可证
本项目基于 **MIT 许可证** 开源 - 详情请参阅 [LICENSE](LICENSE) 文件。
