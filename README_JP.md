# VRC Prop Motion
### 多言語対応 / Multi-language
[**English**](README.md) | [**中文**](README_CN.md) | [**日本語**](README_JP.md)

---

VRChatワールド向けの軽量な **UdonSharp** プラグインです。**プロップ（世界オブジェクト）**の**モーション**を正確に制御できます。

カーブを使用した移動・回転シーケンスの定義、物理演算との連携、ドラッグ操作による起動、そしてネットワーク同期をサポートしています。

![ステータス](https://img.shields.io/badge/ステータス-ベータ-blue) ![ライセンス](https://img.shields.io/badge/ライセンス-MIT-green)

## ✨ 機能
* **トランスフォームアニメーション**: 時間経過に伴う位置・回転の制御。
* **カーブエディタ**: UnityのAnimation Curveを利用したスムーズなイージング。
* **物理演算**: Rigidbodyとの連携（オプション）。
* **ドラッグトリガー**: VRChat標準の `Drag` コンポーネントからアニメーションを起動。
* **ネットワーク同期**: UdonSyncedによるクライアント間の状態同期。
* **エディタ多言語化**: Inspector UIは英語、日本語、中国語に対応。

## 📦 インストール
1. プロジェクトに **UdonSharp** が導入されていることを確認してください。
2. このリポジトリをダウンロードします。
3. `VRCPropMotion` フォルダをUnityプロジェクトの `Assets` フォルダ内にコピーします。
4. *(オプション)* AsmDefを使用している場合は、`Editor` フォルダをEditor用アセンブリに配置してください。

## 🚀 クイックスタート
1. ワールド内に空のGameObjectを作成します。
2. **`PropMotion`** コンポーネントを追加します。
3. **Target**（動かしたいオブジェクト）をアサインします。
4. **Motion Settings**（時間、カーブ等）を設定します。
5. **起動方法**:
   - **Udonイベント**（例: `Interact`, `OnTriggerEnter`）から `StartMotion()` を呼び出します。
   - または、**`DragTriggerRelay`** コンポーネントを追加し、`Drag` コンポーネントと連携させます。

## 🛠️ Inspector リファレンス
### コア (Core)
* **Target**: アニメーションさせるGameObject。
* **Duration**: モーションの合計時間。
* **Start On Load**: ワールド読み込み時に自動再生します。

### モーション (Motion)
* **Move From/To**: 開始・終了位置。
* **Use Local Space**: ローカル座標とワールド座標を切り替えます。

### 回転 (Rotation)
* **Rotate From/To**: 開始・終了時の回転値。
* **Rotation Mode**: `None` (回転しない)、`Linear` (線形)、`Curve` (カーブ)から選択。

### ネットワーク (Networking)
* **Sync Motion**: 有効にすると、モーションの状態が全クライアント間で同期されます。

## 📜 ライセンス
このプロジェクトは **MITライセンス** の下で公開されています - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。
