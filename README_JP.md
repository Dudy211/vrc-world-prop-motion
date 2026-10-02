# VRC World Prop Motion

[![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/Language-中文-red.svg)](README_CN.md)
[![日本語](https://img.shields.io/badge/Language-日本語-green.svg)](README_JP.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![VRChat](https://img.shields.io/badge/VRChat-UdonSharp-orange.svg)](https://vrchat.com)

**VRC World Prop Motion** は、**VRChatワールド**向けの強力な **UdonSharp** プラグインです。コードを書くことなく、プロップの移動・回転・物理演算・ドラッグ操作・ホバーフィードバック・ネットワーク同期を高度に制御できます。

エレベーター、引き戸、精密ダイアル、物理トラック、ドミノ連鎖、近接トリガー機構、インタラクティブ展示などに最適です。

## ✨ 主な機能

### 🔄 Sender / Receiver アーキテクチャ
*   **Receiver (受信側)**: 移動するオブジェクトにアタッチ。変換を実行。
*   **Sender (送信側)**: ボタン/ハンドルにアタッチ。Receiver をトリガー。
*   3つのトリガーモード：**Interact**（クリック）、**Drag**（掴む）、**Passive**（プレイヤー進入）。

### 🚶 Passive / 近接トリガー
*   **Passive モード**：Receiver 周囲に `BoxCollider` トリガーゾーンを自動で配置。
*   プレイヤーがゾーンに**入った時**に移動開始（テレポート/スポーン時は発動しません）。
*   ゾーンの中心とサイズをローカル空間で設定可能。

### 🖱️ ホバーフィードバックシステム
*   **Native**：VRChat 標準のインタラクションプロンプトを使用（Interactive レイヤーが必要）。
*   **Custom**：独自の視覚表現を完全制御。
    *   **Hover Visual**：ローカルプレイヤーが照準を合わせた時に GameObject を表示/非表示。
    *   **Hover Highlight**：`MaterialPropertyBlock` でターゲットレンダラーを着色（クライアントごと、マテリアル複製なし）。
    *   **Hover Text**：エディタが自動生成するワールド空間テキスト（オブジェクト上部に配置）。
    *   距離、点滅速度、色、フォント、オフセットを設定可能。

### 📐 高度なモーション
*   **移動 (Move)**: 直線またはカーブパス（AnimationCurve でアーチ高を制御）。
*   **回転 (Rotate)**:
    *   **球面回転 (Spherical)**: 2つの向きを補間。
    *   **軸回転 (Axis)**: カスタム軸周りを回転（手動/オブジェクト一致/オイラー角）。多回転対応（3600°ダイアル等）。
*   **パスポイント**: 複数の Transform を順に通過。閉ループオプション付き。

### 🎢 カーブと物理演算
*   **カーブ移動**: AnimationCurve でアーチ高と速度イージングを定義。
*   **重力と抵抗**: レール上の現実的な物理（重力で加速、抵抗で減速）。
*   **速度モード**: 固定速度またはカーブ駆動の可変速度。
*   **SnapBack**: 離した後に設定速度で始点に自動復帰。

### 🔗 連鎖とイベント
*   **ドミノ連鎖**: 終点/始点到達時に他 Drag をトリガー（`nextOnReach` / `nextOnReturn`）。
*   **パスポイントイベント**: 各経由点で UdonBehaviour の `OnPathPoint()` を呼び出し。
*   **コールバック**: `OnReachDestination()`、`OnReachStart()`、`OnGrabbed()`、`OnReleased()`。
*   **連鎖マスタースイッチ**: 全イベント/連鎖を一括無効化。

### 🔊 サウンドエフェクト
*   AudioSource 統合（オプション）。
*   目的地到達、始点復帰、掴む、離すの4種のサウンド。
*   サウンドはローカルのみ再生。

### 🌐 ネットワーク同期
*   `UdonBehaviourSyncMode.Continuous`。`syncEveryNFrames` と `syncMinDelta` を設定可能。
*   非オーナー側は受信時に滑らかな補間表示。
*   操作時に自動で所有権を移転。
*   **LockWhileHeld**: 掴まれている間は他プレイヤーのトリガーを禁止。

### 🎨 エディタ拡張
*   **多言語対応**: 英語・日本語・中国語。
*   **シーンビューツール**:
    *   軸の始点/終点/中点ハンドル（シーン上でドラッグ）。
    *   カーブ原点ハンドル（黄色球、自由/単軸ドラッグ）。
    *   世界軸アラインボタン（X/Y/Z）。
    *   始点〜終点間のゴーストメッシュ/ワイヤーフレームスライス。
    *   連鎖関係プレビュー（上流/下流 Drag の可視化）。
*   **スクラブプレビュー**: 進行度（0〜1）をスライドして姿勢をプレビュー。姿勢基準の設定/適用。
*   **自動初期化**: インポート時に `Drag.asset` を自動生成。
*   **内蔵ヘルプ**: クイックスタート、アセット情報、同期解説、トラブルシューティング。

## 📦 インストール

1.  **VCC** 経由で **UdonSharp**（Worlds SDK に同梱）がインストール済みであることを確認。
2.  [Releases](https://github.com/Dudy211/vrc-world-prop-motion/releases) から `vrc-world-prop-motion.unitypackage` をダウンロード。
3.  Unity にインポート。`Drag.asset` が生成されない場合は **`Tools > VRC Prop Motion > Setup Drag Program Asset`** を実行。

## 🚀 クイックスタート

### 受信側（動くオブジェクト）
1.  動かすオブジェクトに `Drag.cs` をアタッチ。
2.  **Role** → `Receiver`、`Motion Mode` → `Move`。
3.  **Target** を自身に設定（またはデフォルトのまま）。
4.  **Destination Mode** を設定（Offset Vector / Destination Transform / Path Points）。
5.  Interact、Drag、または Passive でトリガー。

### 送信側 + 受信側（引き戸）
1.  **Receiver**: ドアにアタッチし、目的地を設定。
2.  **Sender**: ドアノブにアタッチ。**Role** → `Sender`、Receiver を指定。
3.  **Trigger Move Mode**: `Rail`（軌道上をスライド）または `SyncTarget`（ドアに追従）。
4.  ゲーム内でノブを掴んでドアを開閉。

### Passive トリガー（近接ゾーン）
1.  機構オブジェクトに Receiver を配置。**Interaction Mode** → `Passive`。
2.  **Trigger Zone Center** と **Size** を設定（BoxCollider を自動配置）。
3.  プレイヤーがゾーンに入ると自動開始。

### ホバーフィードバック
1.  **Hover Mode** → `Custom`。
2.  **Hover Visual** を指定（照準時の表示オブジェクト）。
3.  **Hover Highlight**（レンダラー着色）または **Hover Text**（ワールド空間テキスト）を有効化。

### 軸回転（ダイアル/バルブ）
1.  ダイアルに Receiver を配置。**Motion Mode** → `Rotate`、`Rotate Mode` → `Axis`。
2.  **Axis Source** → `Object Align`（ダイアルのローカルY軸に一致）。
3.  **Axis Angle** → `360`（または `720`、`3600` で多回転）。
4.  Sender（ハンドル）を追加し、Drag 操作を設定。

## ⚠️ トラブルシューティング

| 問題 | 解決法 |
|------|--------|
| `Program Source is None` | `Tools > VRC Prop Motion > Setup Drag Program Asset` を実行 |
| `Drag.cs referenced by 2 UdonSharpProgramAssets` | `Drag.asset` を削除して既存参照を使用、または `Drag.cs` をリネーム |
| ドラッグが反応しない | Trigger に `VRC_Pickup` + `Collider` + `Rigidbody` が必要 |
| シーンの軸ハンドルが掴みにくい | 中心ではなく XYZ の矢印をドラッグ、または先に Inspector でオフセット入力 |
| Passive モードが発動しない | 自動生成された BoxCollider が有効で十分な大きさか確認 |
| Drag.cs フィールド編集後に Inspector がおかしい | `Drag.asset` + `.meta` を削除し、Tools メニューから再生成 |

## 📜 ライセンス
MIT License — 詳細は [LICENSE](LICENSE) をご覧ください。
