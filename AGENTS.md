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
| カード | 同レアリティ・同キーワード他カード、eng/jpn キー、CSV タグ／数値 |
| パワー／パッチ | `Powers/`・`Patches/` の同フック（Harmony 型・カウント制限のデグレ） |
| 心臓レリック | `HeartRegistry`・`HeartCapture`・`Relics/Hearts/` |
| 多段／リプレイ | `WithHitCount` / `BaseReplayCount`（手動ループ禁止） |
| プレビュー | 説明・ホバー・実効が同一計算か |
| アセット | `dotnet publish` で `.pck` 更新（build だけではローカライズが載らない） |

## 蓄積知見

### 「更新して」＝ゲーム反映コマンド一式

ユーザーが「更新して」「反映して」などと言ったときは、描画・文言・ローカライズ・アセット系の反映として次を実行する（報告前に済ませる）。

```powershell
# リポジトリルート（StSMod_N）で
dotnet publish
```

- `dotnet build` … コード（dll）だけ。文言・画像は古いままになりやすい
- `dotnet publish` … dll + `.pck`（ローカライズ／画像／シーン）。**普段はこちら**
- 反映後はゲームを再起動する（起動中のままでは古い dll / pck のまま）
- 出力先: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods/HypnosisCreator/`（`Directory.Build.props` の `Sts2Path` 準拠）

キー名のまま表示される／アイコンが古いときは、ほぼ確実に `.pck` 未更新かゲーム未再起動。

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
- **色の基準（本家 StS2 準拠）**:
  - `[gold]…[/gold]` … キーワード名・固有名詞（Block／Strength／Exhaust／Trance／沼／破滅 等）
  - `[blue]…[/blue]` … **文中の固定数値**（`1`／`3`／`20%`／`50%` など）。本家の脱力・弱体・残像・悪魔化などと同じ
  - カードの可変数値（UGで緑差分が出るもの）は `[blue]` ではなく `{Damage:diff()}` / `{Block:diff()}` 等
- Power: `description` は固定例の数値を `[blue]`、`smartDescription` は同じ構文で `{Amount}` 化（残像／激怒型）。スタック表示だけの Amount は文中に無理に入れない
- 日本語はだ体（`〜を得る。`／`〜する。`）。ですます禁止
- 性癖パワーの定型: `性癖に刺さる行動を受けた時、[gold]破滅[/gold][blue]{Amount}[/blue]を得る。`（eng: `When receiving an action that hits Fetish, gain [blue]{Amount}[/blue] [gold]Doom[/gold].`）
- 例（ふにゃへにゃ）: `[gold]トランス[/gold][blue]1[/blue]につき…[blue]20%[/blue]減少` → キーワードは金、固定数値は青で正しい
- ローカライズ変更後は `dotnet publish`（build だけでは `.pck` に載らない）

### CSV効果説明を正本（意訳禁止）

- カード `効果説明` の正本はユーザー提供 CSV。エージェントが勝手に言い換え・効果追記しない
- loc 本文は CSV の語順・語彙に合わせる（`[gold]` / `{Damage:diff()}` 等のマークアップのみ許容）
- 可変括弧 `（Nダメージ）` 等は **loc に書かず**、戦闘時のみ `CombatPreviewText` / `CombatDamageSuffixPreview` で末尾追記
- パワーのスタック倍率はカード文に書かず、アイコン数字＋`smartDescription` の `{Amount}`

### 戦闘時ダメージ括弧プレビュー

- 末尾括弧の数値は `CardDamagePreview.ApplyModifiers`（実ダメージと同じ Hook）
- ベースと異なれば `FormatPreviewAmount` で緑表示
- 全体攻撃の合計は `CombatDamageSuffixPreview.ResolveAoEPerHit`（全員同型デバフ時のみ敵側補正、混在時はベース寄り）

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
- Exhaust / Retain 等は `CanonicalKeywords` で付ける。**説明文末に「廃棄。」／`Exhaust.` を書かない**（キーワードが自動挿入するため「廃棄。廃棄。」になる）。調和・ミラーリング側に合わせる
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

### StS2キャラmod開発 — 再発防止ベストプラクティス

コミット履歴・バグ修正・実装コメントから抽出。  
ローカライズ文体・UG差分・Defend／敏捷・カード追加手順は **上記セクションを正** とする（ここでは重複しない）。  
確定メカニクスの数値・条件は `mechanics-lock.md` と CSV を正とする。

#### 1. 戦闘フリーズ・宙吊り（最優先で避ける）

| 失敗 | 原因 | やり方 |
| ---- | ---- | ------ |
| 戦闘開始で固まる | 表示付き `PowerCmd.Apply` を `GetResult` 等で待つと `CustomScaledWait` がゲームループを塞ぐ | 開幕の性癖付与などは **待たない／非同期**。表示不要なら silent／非表示 |
| 多段攻撃で宙吊り | `Execute` 内の手動ヒットループ | **`WithHitCount(n)` 1回**で解決（`Mirroring` / `InfiniteFingerSnap` 参照） |
| リプレイ系で戦闘停止 | 独自の再プレイ実装 | **`BaseReplayCount`** と本家と同じ解決経路。説明文に「リプレイ」を書くと自動追記と二重になる |
| 睡眠でターンが進まない／終了宙吊り | 本家 `AsleepPower` と違う進行・意図復元忘れ | 睡眠中の行動予定表示・解除後の意図復元・ビートル同様のダメージ減衰を本家に合わせる |

#### 2. Harmony パッチ（1つ壊すと全体が死ぬ）

- パッチの引数型・`out` 有無が本家と1つでも違うと **そのパッチだけでなく後続 Harmony も止まる**（例: アニメ長が `float` なのに別型で刺した）
- `CardModel.CanPlay` は **out 引数あり／なしの両方**がある。カウント制限は両方＋`CanPlayTargeting` を見る（`CountUnplayablePatch`）
- パッチ追加・シグネチャ変更後は「カウントがトランスなしで打てない」「コスト−1が効く」など **既存パッチの実機確認**をセットにする
- 型ごとに適用し、1失敗で連鎖死しない書き方を優先する

#### 3. 心臓レリック

- ID解決・型解決は **`HeartRegistry` に集約**。散在する自前マップを増やさない
- `CustomRelicModel` は `Activator.CreateInstance` 不可 → **`ModelDb.AllRelics` の登録済みインスタンス**を使う。0件マップだと全敵が「奪った心臓」に落ちる
- ドロップは原則 **報酬画面の追加 `RelicReward`**（`HeartCapture.TryAddExtraRelicReward`）。即時 Obtain は部屋が無いときのフォールバックのみ（`mechanics-lock.md`）
- 同一心臓を共有する複数体（ムカデ等）は **最後の1体撃破時だけ**落とす
- 心停止・寄生などは **付与時にモンスターIDを保存**してから解決する（倒れた瞬間の解決だと取り違える）
- 自己暗示など一時所持は **戦闘終了で必ず消す**（残ると永久所持バグ）

#### 4. カウント／トランス／手札戻し

- コスト−1は **手札ドロー前**だけ（`mechanics-lock.md`）。開幕手札が減って見えるのはバグ
- トランス解除条件（単体／全体／自分）は lock 表どおり。パッチを触ったら **制限が消えていないか**を必ず確認（何度もデグレした）
- 「廃棄の代わりに手札」は `AfterCardPlayed` では遅い → 本家 Rebound 同様 **`ModifyCardPlayResultLocation`**（`EncorePower`）
- アンコール×カウントなど横断組み合わせは、片方直したらもう片方も見る

#### 5. プレビューと実効を同じ計算にする

- カード説明・ホバー・黄ハイライト・実ダメージ／ブロックは **同一の計算関数**を使う
- 失敗例: 調和の `GetTotalDamage` 引数誤り、期待に応えての2乗、弱体プレビューずれ、ゼロへの近道の合計未連動
- `CalculatedVar` / `CalculatedDamageVar` は **`Base + Extra × Func`**。プレビュー用なら `CalculationBaseVar(0)`＋`CalculationExtraVar(1)`（または `ExtraDamageVar(1)`）。`Extra=0` だと表示が常に0になる
- **戦闘状況依存の括弧プレビューは戦闘中だけ**（本家 `CalculatedVar.Calculate` は非戦闘で倍率スキップ→0）。説明文に `{Draw:diff()}` 等を埋めず、`CombatPreviewText` で付与する（不意打ち・催眠導入・連続指パッチン等）。攻撃カードの可変ダメージは本家 MindBlast 同様、枠のダメージ数字（`CalculatedDamageVar`）に任せ、説明への「(0ダメージ)」併記はしない
- 戦闘外でも意味がある数値（所持心臓数・固定合計ブロックなど）は戦闘外表示してよい
- 「性癖が一致するだけで黄ハイライト」は禁止。**今プレイ可能なときだけ**（条件未達の時止め等）

#### 6. 仕様正本とタグ

- カード効果・数値・性癖タグ・UGの正本は **CSV**。実装前に突合する（タグ欠落／過剰が何度も発生）
- スターター攻撃に **`CardTag.Strike` を安易に付けない**（本家スターター方針とズレる）
- 自己バフ／敵デバフを取り違えない（エリクソン＝自己、推し活＝敵 等）。CSV備考を読む
- マルチ専用は `MultiplayerOnly`。ソロの巻物箱・報酬抽選から除外

#### 7. 報酬・ゴールド・パワー付与

- 戦闘終了ゴールドは所持金直加算ではなく、本家 Royalties 同様 **報酬画面の `GoldReward`**
- スタック付きパワーを毎プレイ `Apply` し直すと **内部予約が消える**ことがある → 未所持のときだけ Applyし、以降は既存インスタンスを更新（深淵からの声援）

#### 8. 実装前チェック（短く）

新しい攻撃・パッチ・心臓・カウント変更の前に:

1. 本家に同型があるか（`WithHitCount` / Rebound / Royalties / GeneticAlgorithm 等）
2. `mechanics-lock.md` と CSV に反していないか
3. 同フォルダ・同フックの水平展開（特に `Patches/`・`Relics/Hearts/`・カウント）
4. ローカライズ／画像を触るなら `dotnet publish`（build だけではキー名表示）
