# Hypno Creator — エージェント知見（正本）

人間が承認した知見のみ追記する。ルール再分割は確認後。

## 完了前の必須事項

- コード変更ありの完了報告・commit / push の前に **C2 セルフレビュー必須**（水平展開・再発防止を含む）
- 詳細手順は `.cursor/rules/composer-self-review.mdc`
- 完了チェックリスト・返信形式は `.cursor/rules/hypnosis-creator.mdc`
- 知見の提案フローは `.cursor/rules/skills_collector.mdc`（**承認前に本ファイルを書き換えない**）

## 正本の役割分担

| ファイル | 役割 |
| -------- | ---- |
| `AGENTS.md`（本ファイル） | エージェント運用・再発防止の知見 |
| `mechanics-lock.md` | 確定ゲームメカニクス（ユーザー指示時のみ変更） |
| `task.md` | 未完了タスクのみ |
| `README.md` | セットアップ・導線 |

## 水平展開（クイック参照）

| 変更の種類 | 調査例 |
| ---------- | ------ |
| カード | 同レアリティ・同キーワード他カード、eng/jpn キー |
| パワー／パッチ | `Powers/`・`Patches/` の同フック |
| 心臓レリック | `Relics/Hearts/`・`HeartInventory` |
| アセット | `dotnet publish` で `.pck` 更新（build だけではローカライズが載らない） |

## 蓄積知見

### ローカライズ文体（本家 StS2 準拠）

カード／パワー／レリックの説明文は、本家 `SlayTheSpire2.pck` の `localization/{eng,jpn}/powers.json` に合わせる。参照抽出: `_vanilla_loc_extract/sts2/localization/`。

- **完全な一文**で書く（`Whenever…` / `〜するたび` / `ターン開始時`）。タイトル繰り返し・電報調（`ドロー3。`／`性癖必中。`／`名前 {Amount}。`）は禁止
- 数値は `[blue]…[/blue]`、キーワードは `[gold]…[/gold]`（Block／Exhaust／Strength、本modなら Count・Trance・Bog・Doom 等）
- Power: `description` は固定例の数値、`smartDescription` は同じ構文で `{Amount}` 化（残像／激怒型）。スタック表示だけの Amount は文中に無理に入れない
- 日本語はだ体（`〜を得る。`／`〜する。`）。ですます禁止
- 性癖パワーの定型: `性癖に刺さる行動を受けた時、[gold]破滅[/gold][blue]{Amount}[/blue]を得る。`（eng: `When receiving an action that hits Fetish, gain [blue]{Amount}[/blue] [gold]Doom[/gold].`）
- ローカライズ変更後は `dotnet publish`（build だけでは `.pck` に載らない）

### UIアイコンの縁（レリック／バフ・デバフ）

- 暗いUI向けアイコンで、外側の明るい縁・ハロー・ステッカー枠は避ける（背景から浮いて見える）
- 輪郭は暗いアウトライン、またはモチーフ色の細いソフトリム＋アンチエイリアスで馴染ませる
- 主体が暗い色のときは白縁で形を取らず、同系色のリムや内側ハイライトで視認性を確保する
