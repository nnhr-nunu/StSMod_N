# Hypno Creator

Slay the Spire 2 のキャラクターmod。活動名「ぬぬはら」を原案にした **Hypno Creator**。

依存: [BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)

## 三軸

| 軸 | 概要 |
|----|------|
| **カウント** | 高コスト・保留。ターン開始でコスト−1。0のときプレイ可（対象がトランスなら非0でも可）。廃棄 |
| **トランス／性癖** | 敵にトランスを積みカウント制限を解除。性癖が刺さると破滅（`DoomPower`）。沼で破滅増幅 |
| **心臓** | リーサル等で心臓レリックを奪う（現状は汎用 `StolenHeart`、個別心臓は拡張予定） |

確定ルールの正本: [`mechanics-lock.md`](mechanics-lock.md)

## セットアップ

依存: Steam Workshop の **BaseLib**（必須）

```bash
# .NET 9 + Godot/MegaDot 4.5.1 mono 済み想定
cp Directory.Build.props.example Directory.Build.props
# Windows 例: Sts2Path / GodotPath を Directory.Build.props に書く
dotnet build          # コードのみ → mods/HypnosisCreator/ へ dll コピー
dotnet publish        # 画像・シーン込み（.pck 生成）
```

出力先:
- Windows: `…/Steam/steamapps/common/Slay the Spire 2/mods/HypnosisCreator/`
- macOS: `…/SlayTheSpire2.app/Contents/MacOS/mods/HypnosisCreator/`

ゲーム内で **BaseLib** と **Hypno Creator** を有効化。

## 主なディレクトリ

- `HypnosisCreatorCode/` … C#（キャラ・カード・レリック・パッチ）
- `HypnosisCreator/` … 画像・ローカライズ（eng / jpn）
- `HypnosisCreator.json` … mod マニフェスト
- `card-asset-template.md` … **カード名・効果・サムネの受け渡し用テンプレート**

詳細: [ModTemplate-StS2 Wiki](https://github.com/Alchyr/ModTemplate-StS2/wiki)
