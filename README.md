# Hypnosis Creator（催眠制作者）

Slay the Spire 2 のキャラクターmod。活動名「ぬぬはら」を原案にした **Hypnosis Creator**。

依存: [BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)

## 三軸

| 軸 | 概要 |
|----|------|
| **カウント** | 基本高コスト。解決後コストが0のときだけプレイ可（Retain／コスト操作あり） |
| **トランス** | Dom（自分）と Sub（敵）を積み、一致で割合ダメージ／不一致で敵回復 |
| **心臓** | リーサルで心臓レリックを奪う（現状は汎用 `StolenHeart`、個別心臓は拡張予定） |

## セットアップ

```bash
# .NET 9 + MegaDot 済み想定
cp Directory.Build.props.example Directory.Build.props
export PATH="$HOME/.dotnet:$PATH"
dotnet build          # コードのみ
dotnet publish        # 画像・文言込み（.pck 生成）
```

出力先（macOS）:  
`…/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/HypnosisCreator/`

ゲーム内で **BaseLib** と **Hypnosis Creator** を有効化。

## 主なディレクトリ

- `HypnosisCreatorCode/` … C#（キャラ・カード・レリック・パッチ）
- `HypnosisCreator/` … 画像・ローカライズ（eng / jpn）
- `HypnosisCreator.json` … mod マニフェスト

詳細: [ModTemplate-StS2 Wiki](https://github.com/Alchyr/ModTemplate-StS2/wiki)
