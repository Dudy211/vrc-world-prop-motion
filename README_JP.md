# VRC World Prop Motion

[![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/Language-中文-red.svg)](README_CN.md)
[![日本語](https://img.shields.io/badge/Language-日本語-green.svg)](README_JP.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)
[![Latest Release](https://img.shields.io/github/v/release/Dudy211/vrc-world-prop-motion?include_prereleases)](https://github.com/Dudy211/vrc-world-prop-motion/releases)

**VRC World Prop Motion** は、**VRChatワールド**向けの強力な **UdonSharp** プラグインです。
コードを書くことなく、インスペクター上の設定だけで、プロップ（オブジェクト）の移動・回転・物理演算・ネットワーク同期を高度に制御できます。

エレベーター、引き戸、精密なダイアル、物理トラックなどの実装に最適です。

## ✨ 主な機能

### 🔄 Sender / Receiver アーキテクチャ
*   **Receiver (受信側)**: 移動するオブジェクトにアタッチし、実際の変換処理を実行します。
*   **Sender (送信側)**: ボタンやハンドルにアタッチし、Receiver をトリガーします。
*   **Interact** (クリック) と **Drag** (手動操作) の両方のトリガーモードをサポート。

### 📐 高度なモーション制御
*   **移動 (Move)**: ポイント間の直線移動。
*   **回転 (Rotate)**:
    *   **球面回転 (Spherical)**: 2つの向きの間を滑らかに補間。
    *   **軸回転 (Axis)**: カスタム軸（手動、オブジェクト基準、オイラー角）周りを回転。多回転（例: 3600°のダイアル）にも対応。

### 🎢 パスとカーブ
*   **パスポイント (Path Points)**: 複数の Transform を順番に通過。
*   **アニメーションカーブ (AnimationCurve)**: 移動時の高さの弧や速度のイージングを定義。
*   **自動原点**: パスの中点を自動計算し、カーブの基準点として設定。

### 🛠️ 物理演算とインタラクション
*   **レールモード (Rail)**: ドラッグを特定のトラックに拘束（引き戸などに最適）。
*   **同期ターゲット (SyncTarget)**: ハンドルがオブジェクトに追従し、精密な操作が可能。
*   **重力と抵抗**: 現実的なスライド物理演算をシミュレート。
*   **ループとピンポン**: 柔軟なパスの繰り返し設定。

### 🌐 ネットワーク同期
*   `UdonBehaviourSyncMode.Continuous` を使用した安定した同期。
*   オーナーシップ（所有権）の自動移転処理。

### 🎨 エディタ拡張
*   **多言語対応**: インスペクターが **English / 日本語 / 中文** をサポート。
*   **シーンビューギズモ**:
    *   目的地やパスのプレビュー表示。
    *   回転軸や移動軌道の可視化。
    *   シーンビュー上で直接ベクトルを調整可能なドラッグハンドル。
*   **自動初期化**: インポート時に `Drag.asset` を自動生成し、UdonSharp の参照エラーを防止。

## 📦 インストール

1.  **VCC (VRChat Creator Companion)** 経由で **UdonSharp** (Worlds SDK に同梱) がインストールされていることを確認してください。
2.  [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases) ページから最新の `vrc-world-prop-motion.unitypackage` をダウンロードします。
3.  Unity プロジェクトにインポートします。
4.  `Drag.asset` が自動生成されなかった場合は、メニューの **`Tools > VRC Prop Motion > Setup Drag Program Asset`** を実行してください。

## 🚀 クイックスタート

### オブジェクトを動かす (Receiver)
1.  動かしたいオブジェクトに `Drag.cs` をアタッチします。
2.  **Role** を `Receiver` に設定します。
3.  **Motion Mode** を `Move` に設定します。
4.  **Destination Mode** (例: Offset Vector) と **Offset** の値を調整します。
5.  Play モードに入るか、Interact/Drag でトリガーします。

### 引き戸を作る (Sender + Receiver)
1.  **Receiver**: ドアにアタッチし、終点位置を設定します。
2.  **Sender**: ドアノブにアタッチします。
    *   **Role**: `Sender`
    *   **Receiver**: ドアオブジェクトを割り当てます。
    *   **Interaction Mode**: `Drag`
    *   **Trigger Move Mode**: `Rail` (レール沿いにスライド)。
3.  ゲーム内でノブを掴んでドアを開け閉めします。

## ⚠️ トラブルシューティング

### 1. `Program Source is None` エラー
*   `Assets/VRCPropMotion/Drag.asset` が存在するか確認してください。
*   存在しない場合は、メニューの `Tools > VRC Prop Motion > Setup Drag Program Asset` を実行してください。

### 2. `Drag.cs is referenced by 2 UdonSharpProgramAssets`
*   **原因**: 他のプラグイン（例: Ohmiwa のサンプル等）が同名の `Drag.cs` を使用しているため、競合が発生しています。
*   **対処法**:
    *   `Assets/VRCPropMotion/Drag.asset` を削除し、既存の参照を使用する。
    *   または、本プラグインの `Drag.cs` を `VRCPropMotionDrag.cs` 等にリネームしてください（Editor スクリプトの修正も必要です）。

## 📜 ライセンス
本プロジェクトは **MIT License** の下で公開されています。詳細は [LICENSE](LICENSE) ファイルをご覧ください。

---
💡 **Note**: このツールが役に立った場合は、Star をいただけると嬉しいです。バグ報告や機能提案は Issues までお願いします。
