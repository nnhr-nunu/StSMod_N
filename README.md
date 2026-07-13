# NewCharMod（Slay the Spire 2 キャラクターmod）

BaseLib 依存のキャラクター追加テンプレートから生成した開発用プロジェクトです。

## 必要なもの

| 項目 | 状態・場所 |
|------|------------|
| Slay the Spire 2 | Steam: `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2` |
| BaseLib（Workshop） | 購読済み（`workshop/content/2868840/3737335127/BaseLib`） |
| .NET SDK 9+ | `~/.dotnet`（PATH に追加済み想定） |
| MegaDot（アセット出力用） | `~/Applications/MegaDot.app` |

## 初回セットアップ（別マシン向け）

```bash
# .NET
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# MegaDot
curl -fsSL https://megadot.megacrit.com/install.sh | sh

# テンプレート（任意・新規作成時）
dotnet new install Alchyr.Sts2.Templates

# 本リポジトリ
cp Directory.Build.props.example Directory.Build.props
# GodotPath が違う場合は Directory.Build.props を編集
```

Steam Workshop で **BaseLib** を購読してください。

## ビルド / 公開

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build          # コードのみ（.dll を mods へコピー）
dotnet publish        # 画像・文言込み（.pck も生成）
```

出力先（macOS）:

`…/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/NewCharMod/`

ゲーム起動後、設定 → Mod Settings で **NewCharMod** と **BaseLib** を有効化します。

## 主なディレクトリ

- `NewCharModCode/` … C#（キャラ・カード・レリック等）
- `NewCharMod/` … 画像・ローカライズ（文言）
- `NewCharMod.json` … mod マニフェスト

詳細な作り方は [ModTemplate-StS2 Wiki](https://github.com/Alchyr/ModTemplate-StS2/wiki) を参照。
