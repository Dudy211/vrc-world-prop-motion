# VRC World Prop Motion（日本語説明）

[![最新リリース](https://img.shields.io/github/v/release/Dudy211/vrc-world-prop-motion?label=%E6%9C%80%E6%96%B0%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9&color=blue)](https://github.com/Dudy211/vrc-world-prop-motion/releases)
[![ライセンス: MIT](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)
[English](./README.md) · [中文](./README_CN.md) · [日本語](./README_JP.md)

**VRChat ワールド**向けの軽量 **UdonSharp** プラグイン。
**プロップ（ワールドオブジェクト）** に正確なモーション制御を追加します。

カーブ・物理演算・ドラッグ操作で移動 / 回転シーケンスを定義し、
**ネットワーク越しに同期**できます。

---

## ✨ 機能

- **Transform 制御** — 設定可能なパスに沿って移動・回転。
- **カーブと物理** — `AnimationCurve` と重力でモーションを駆動。
- **インタラクション** — `Interact` や **Drag** コンポーネントで発動。
- **ネットワーク同期** — 全クライアント間でスムーズに同期。
- **多言語** — インスペクターは **English · 中文 · 日本語** 対応。

---

## 📦 インストール

### 方法 A：Unitypackage（推奨）

1. [**Releases ページ**](https://github.com/Dudy211/vrc-world-prop-motion/releases)
   から最新の `vrc-world-prop-motion-x.y.z.unitypackage` をダウンロード。
2. Unity で**ダブルクリック**、または `Assets > Import Package > Custom Package…`
   からインポート。
   > ⚠️ **重要：** インポート一覧で各 `.cs` に同名の `.meta` が
   > ペアであることを確認してください（参照が壊れます）。
3. **UdonSharp** が **VCC** で導入済みであることを確認
   （**VRChat Worlds SDK** に同梱）。
4. プラグインは `Assets/VRCPropMotion/` に配置されます。

### 方法 B：Clone / ダウンロード（開発者向け）

1. プロジェクトに **UdonSharp** が導入されていることを確認。
2. リポジトリをクローン、または ZIP をダウンロードし、
   `VRCPropMotion/` フォルダを `Assets/` にコピー。

---

## 🚀 クイックスタート

1. 方法 A でパッケージをインポート。
2. シーンでアニメーションさせたい **プロップ** を選択。
3. `Add Component > Drag`（UdonSharp ビヘイビアー）。
4. インスペクターで**カーブ・物理・インタラクション**を設定。
5. **Play Mode** に入り、モーションとネットワーク同期をテスト。

---

## 🔧 要件

| 依存関係 | バージョン |
|---|---|
| Unity | `2019.4.31f1`（VRChat World テンプレート） |
| VRChat Worlds SDK | 最新（VCC 経由） |
| UdonSharp | 最新（Worlds SDK に同梱） |

---

## 🐛 トラブルシューティング

### `Program Source is None` / asset が自動生成されない
プラグインの **Editor スクリプト** がインポート時に必要な
`UdonSharpProgramAsset`（`.asset`）を自動作成し、`Drag.cs` にバインドします。
- **Console** に `[VRCPropMotion] 已生成 / Generated Drag.asset` ログが
  出ているか確認。
- 実行されなかった場合はメニューから手動で：
  `Tools > VRC Prop Motion > Setup Drag Program Asset`。

### `Script Drag.cs is referenced by 2 UdonSharpProgramAssets`
プロジェクト内の**別アセット**（例: ダウンロードした Ohmiwa の
`SlantedCeilingRoom` プレハブ）が**同じ `Drag.cs` を参照**しています。
UdonSharp は**1スクリプト = 1プログラムアセット**を要求します。

**対処法（いずれか）：**
1. `Assets/VRCPropMotion/Drag.asset` を**削除**し、別アセットに任せる。
2. スクリプト/クラスをユニークな名前（例: `VRCPropMotionDrag`）に
   **リネーム**して衝突回避 —— **配布時はこちらを推奨**。
3. Unity を再起動し、Editor スクリプトに再リンクさせる。

### インポートエラー / meta が壊れる
- 自身の `.unitypackage` に **UdonSharp / VRCSDK** フォルダを
  **含めない**（ユーザーは VCC で導入）。
- **`.meta` ファイルを必ず保持**し、GUID 参照を維持する。

バージョン履歴は [CHANGELOG.md](./CHANGELOG.md) を参照。

---

## 🤝 コントリビュート

Issue や Pull Request は大歓迎！
バグ発見や機能提案は Issue まで。

**早期ベータ版**のため、VRChat / UdonSharp に詳しい方の
ご協力を心よりお待ちしております 🙏

---

## 📝 ライセンス

[MIT](./LICENSE) © 2026 [Dudy211](https://github.com/Dudy211)
