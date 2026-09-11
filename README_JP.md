# VRC World Prop Motion

> **最新リリース：** [v1.0.0](https://github.com/Dudy211/vrc-world-prop-motion/releases/latest) · **ライセンス：** [MIT](LICENSE)

### 多言語対応 / Multi-language
[**English**](README.md) | [**中文**](README_CN.md) | [**日本語**](README_JP.md)

---

VRChatワールド向けの軽量な **UdonSharp** プラグインです。**プロップ（世界オブジェクト）**の**モーション**を正確に制御できます。

カーブを使用した移動・回転シーケンスの定義、物理演算との連携、ドラッグ操作による起動、そしてネットワーク同期をサポートしています。

## ✨ 機能
- **トランスフォームアニメーション**：時間経過に伴う位置・回転の制御。
- **カーブエディタ**：UnityのAnimation Curveを利用したスムーズなイージング。
- **物理演算**：Rigidbodyとの連携（オプション）。
- **ドラッグトリガー**：VRChat標準の `Drag` コンポーネントからアニメーションを起動。
- **ネットワーク同期**：UdonSyncedによるクライアント間の状態同期。
- **エディタ多言語化**：Inspector UIは英語、日本語、中国語に対応。

## 📦 インストール

### 方法 A：Unitypackage（推奨）
1. [**Releases ページ**](https://github.com/Dudy211/vrc-world-prop-motion/releases) から最新の `vrc-world-prop-motion-x.y.z.unitypackage` をダウンロードします。
2. Unityプロジェクトにインポートします：
   - `.unitypackage` ファイルをダブルクリック、**または**
   - メニュー `Assets > Import Package > Custom Package...` からファイルを選択。
3. **UdonSharp** がインストールされ、最新であることを確認してください。

### 方法 B：Clone / ダウンロード（開発者向け）
1. プロジェクトに **UdonSharp** が導入されていることを確認してください。
2. このリポジトリをClone、またはZIPをダウンロードします。
3. プラグインフォルダを `Assets` ディレクトリにコピーします（全ての `.meta` ファイルを保持）。

## 🚀 クイックスタート
1. ワールド内に空のGameObjectを作成します。
2. **`PropMotion`** コンポーネントを追加します。
3. **Target**（動かしたいオブジェクト）をアサインします。
4. **Motion Settings**（時間、カーブ等）を設定します。
5. **起動方法**：
   - **Udonイベント**（例: `Interact`、`OnTriggerEnter`）から `StartMotion()` を呼び出します。
   - または、**`DragTriggerRelay`** コンポーネントを追加し、`Drag` コンポーネントと連携させます。

## 🛠️ Inspector リファレンス

### コア (Core)
- **Target**：アニメーションさせるGameObject。
- **Duration**：モーションの合計時間。
- **Start On Load**：ワールド読み込み時に自動再生します。

### モーション (Motion)
- **Move From / To**：開始・終了位置。
- **Use Local Space**：ローカル座標とワールド座標を切り替えます。

### 回転 (Rotation)
- **Rotate From / To**：開始・終了時の回転値。
- **Rotation Mode**：`None`（回転しない）、`Linear`（線形）、`Curve`（カーブ）から選択。

### ネットワーク (Networking)
- **Sync Motion**：有効にすると、モーションの状態が全クライアント間で同期されます。

## 📋 動作環境
- **UdonSharp** v1.0 以降
- **Unity** 2019.4.31f1（VRChat World Starter Template）

## 🤝 コントリビュート
Issue や Pull Request は大歓迎です。大きな変更は事前に Issue でご相談ください。

## 📜 ライセンス
このプロジェクトは **MITライセンス** の下で公開されています - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。
