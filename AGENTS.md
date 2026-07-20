# Hypno Creator — エージェント知見（正本）

人間が承認した知見のみ追記する。ルール再分割は確認後。

## 完了前の必須事項

- コード変更ありの完了報告・commit / push の前に **C2 セルフレビュー必須**（水平展開・再発防止を含む）
- 詳細手順は `.cursor/rules/composer-self-review.mdc`
- 完了チェックリスト・返信形式は `.cursor/rules/hypnosis-creator.mdc`
- 知見の提案フローは `.cursor/rules/skills_collector.mdc`（**承認前に本ファイルを書き換えない**）
- **モデル既定は Grok / Composer。高級モデルはユーザー明示指示があるときだけ**（詳細は下記・`.cursor/rules/cursor-model-routing.mdc`）

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

### モデル運用（Grok / Composer 既定・高級モデルは許可制）

失敗例: チャットログ網羅調査のため、指示なく `Task` に GPT 系高級モデルを指定した。

- **既定は Grok と Composer（C2）のみ**。調査が長くても、独断で GPT / Claude 等へ切り替えない
- モデル選択・切替はユーザー操作。エージェントは「高級モデル向きかも」と **提案するだけ**
- `Task` / サブエージェントでも `model:` に高級モデルを付けない（ユーザーが明示したときだけ）
- 「thorough / 網羅 / 複雑な調査」は高級モデル起動の理由にならない。Grok / Composer のツール（grep・read・git log）で進める
- 詳細の提案基準は `.cursor/rules/cursor-model-routing.mdc`

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

### 新規カード追加・調整（本家 StS2 準拠）

参照: 本家 `sts2.dll` のカード実装、および `_vanilla_loc_extract/sts2/localization/{eng,jpn}/powers.json`。カード本文の抽出は `tools_reflect/pck_*_cards.json` や本家カードクラスを直接参照。

#### 追加チェックリスト

1. `HypnosisCreatorCode/Cards/{Rarity}/YourCard.cs` — `[Pool(typeof(HypnosisCreatorCardPool))]` + `HypnosisCreatorCard(cost, type, rarity, target)`（`autoAdd` 既定のままなら MainFile 登録不要）
2. `localization/eng/cards.json` と `jpn/cards.json` に `HYPNOSISCREATOR-{SNAKE_CASE}` の `.title` / `.description`
3. ポートレート: `images/card_portraits/{snake}.png` と `big/{snake}.png`（クラス名の snake_case）
4. 定性UGなら `UpgradeDescriptionHooks` に追記（`.upgradeDescription` キーは使わない）
5. ローカライズ／画像変更後は `dotnet publish`（`build` だけでは `.pck` に載らない）

#### 効果文・プレースホルダ

| 用途 | 書き方 |
| ---- | ------ |
| カード数値（UGで緑差分） | `{Block:diff()}` / `{Damage:diff()}` / `{Cards:diff()}` / 独自なら `{Doom:diff()}` 等。名前は `DynamicVar` 名と一致 |
| 固定数値（文中の定数） | `[blue]2[/blue]` |
| キーワード | `[gold]Block[/gold]` / `[gold]Exhaust[/gold]` / 本modなら Count・Trance・Bog・Doom 等 |
| Power の `description` | 固定例の数値（`[blue]3[/blue]`） |
| Power の `smartDescription` | 同じ文で `{Amount}` 化（スタック量が文意に効くときだけ） |
| 日本語 | だ体。ユーザー指定の文面を優先し、構造だけ本家／既存modに合わせる |

- カード説明に `{Amount}` は使わない（Power 用）。カードは `{VarName:diff()}`
- Exhaust / Retain 等は `CanonicalKeywords` で付ける。説明文に「廃棄。」を書くかは既存カードに合わせる（本modは書く例が多い）
- 定性UGは全文差し替え禁止。`UpgradeCardText.ReplaceWhenUpgraded` / `AppendGreenLine` で差分だけ `[green]`

#### UG の型

| 型 | 実装 |
| -- | ---- |
| 数値強化 | `OnUpgrade` で `DynamicVars.X.UpgradeValueBy(n)`。説明は `{X:diff()}` のみ |
| コスト減少 | `EnergyCost.UpgradeBy(-1)` |
| キーワード付与／除去 | `AddKeyword` / `RemoveKeyword` |
| 効果追加・文言変化 | 空の `OnUpgrade()` + `UpgradeDescriptionHooks` |

#### 本家参考パターン（実装時に開くクラス）

| やりたいこと | 本家クラス | 要点 |
| ------------ | ---------- | ---- |
| 永続自己強化（Block等） | `GeneticAlgorithm` / `TheScythe` | `[SavedProperty]` で永続値。`DeckVersion` にも同様に反映。`AfterDowngraded` で再計算 |
| あらゆる場所から手札へ | `SummonForth`（顕現） | `PlayerCombatState.AllCards` を絞り `CardPileCmd.Add(..., Hand)` |
| 破滅キル時 | `AbstractModel.AfterDiedToDoom`（例: レリック `BookRepairKnife`） | カードは戦闘山札にいるときだけ `ShouldReceiveCombatHooks`（`Pile.IsCombatPile`） |
| 廃棄の代わりに手札 | `Rebound` / 本mod `EncorePower` | `ModifyCardPlayResultLocation` |
| 通常の Block スキル | 本mod `HcDefend` / 本家 `Defend*` | `GainsBlock` + `BlockVar(n, ValueProp.Move)` + `CreatureCmd.GainBlock`（敏捷が乗る） |
| 永続 Block（敏捷あり） | `GeneticAlgorithm` | `BlockVar(CurrentBlock, ValueProp.Move)`。`CardTag.Defend` は付けない |
| 敵攻撃コピーの Block | 本mod `Harmony` | `CreatureCmd.GainBlock(..., ValueProp.Unpowered, ...)`（敏捷が乗らない） |

#### CardTag.Defend と締め直し（Fasten）

本家 `sts2.dll` 調査済み（`FastenPower.ModifyBlockAdditive` / 各 `Defend*`・`UltimateDefend` の `CanonicalTags`）。

- 本家で `CardTag.Defend` が付くのは **各キャラのスターター「防御」と「究極の防御」だけ**（`DefendIronclad` 等 5種 + `UltimateDefend`）
- 締め直し（`FastenPower`）は「ブロックを得るカード全部」ではなく、`card.Tags` に `CardTag.Defend` があるときだけ追加ブロックする（さらに後述の powered block であること）
- **本mod は `HcDefend` にだけ `CardTag.Defend` を付ける**。その他のブロックスキル（深淵からの声援・凝視の光・メトロノーム等）には付けない
- `CardTag.Defend` を安易に付けると、締め直し UG 後に＋6 されて基礎値がおかしく見える（声援 1＋6＝7 の事故）

#### ブロックと敏捷（Dexterity）

本家 `DexterityPower.ModifyBlockAdditive` と `ValuePropExtensions.IsPoweredCardOrMonsterMoveBlock` 調査済み。

- 敏捷が乗る条件: `ValueProp` が **Move あり、かつ Unpowered なし**（`IsPoweredCardOrMonsterMoveBlock`）
- **多くのブロックスキルは敏捷が乗る**。本家も `BlockVar(n, ValueProp.Move)`（防御・Impervious・GeneticAlgorithm 等）。本mod の声援・`HcDefend`・凝視の光なども同じ
- **敏捷が乗らない主な例**:
  - 敵の攻撃値を参照してブロックする類 → `ValueProp.Unpowered`（本mod「調和」）
  - パワー等が固定値でブロックを付与するとき → 本mod では `ValueProp.Unpowered` が多い（例: 性癖理解・無意識の導き）
- 締め直し（Defend タグ）と敏捷（ValueProp）は別判定。タグを外しても `ValueProp.Move` なら敏捷は通常どおり乗る

#### 「デッキにある」の解釈

- 戦闘中はカード実体がデッキと共有／対応するので、戦闘山札（手札・山札・捨て札・廃棄札）上のフックで「所持していれば有効」を満たせる
- 永続変更は本家どおり `SavedProperty` ＋必要なら `DeckVersion` への二重反映
