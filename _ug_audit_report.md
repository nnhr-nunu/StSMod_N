# Hypno Creator BASE/UG 数値監査レポート（No.1–104）

生成日: 2026-07-19  
データ源: `_csv_cards_1_104.json`（CSV effect / ug 列）+ `_card_audit_report.txt`（No→Class マッピング）  
対象: `HypnosisCreatorCode/Cards/**/*.cs` の `CanonicalVars`・`OnPlay`・`OnUpgrade`・`CanonicalKeywords`

> 本監査は **BASE 数値** と **UG 挙動** に限定。レア度・コスト・ローカライズ文言の差異（`_card_audit_report.txt` 既知分）は原則除外。  
> コードは **未変更**（監査のみ）。

---

## サマリー

| 区分 | 件数 | 説明 |
|------|------|------|
| **OK** | **63** | CSV の BASE/UG 数値・キーワード変更がコードと一致（または許容範囲内） |
| **mismatch** | **30** | CSV とコードで数値・効果・UG が明確に乖離 |
| **needs_inference** | **11** | UG 文言が部分的／曖昧、または CSV だけでは解釈が分岐 |

---

## mismatch 一覧（30件）

| No | 名称 | Class | CSV BASE（要点） | CSV UG | コード現状 | 修正案 |
|----|------|-------|------------------|--------|------------|--------|
| 5 | 心停止催眠 | CardiacArrestHypnosis | 3T後心停止（ボス×2） | 心停止時レリック報酬 | UG時 **プレイ直後** に `StolenHeart` 付与 | 心停止 **発動時**（Power側）に報酬。即時付与を削除 |
| 7 | 感度3000倍 | Sensitivity3000 | トランス3、被ダメ **×3000** | コスト1 | `SensitivityPower` が **×3** | 倍率を 3000 に（または CSV が ×3 の誤記なら CSV 修正） |
| 15 | 前戯 | Prefinger | **弱体1+脱力1**、E+1、廃棄 | 弱体2+脱力2 | **Frail1+Weak1**（DrugHypnosis と逆マッピング） | `VulnerablePower(1)` + `WeakPower(1)`、UG で各+1 |
| 16 | ラポール | Rapport | プレイ時カウント+1進行／非攻撃ターン開始+1 | ターン開始 **常に** +1 | プレイ時進行なし。UG `Stacks=2` で常時-2 | OnPlay で手札カウント進行。UG は条件なし+1 |
| 20 | 好き好き催眠 | LoveHypnosis | バフ意図→プレイヤー、トランス1 | バフ **またはブロック** 意図も | `ConfusedPower` 近似 + UG で Weak 追加 | 意図リダイレクト API 実装。UG は条件拡張のみ |
| 29 | ちゃんとできるね？ | CanDoIt | **脱力1+沼1** | 脱力2+沼1 | **トランス2+自ブロック5**、UG トランス3 | Frail1+Bog1 実装へ全面差し替え |
| 33 | 植物寄生催眠 | PlantParasiteHypnosis | **15dmg** + 毎T HP-10 + トランス1 | 毎T HP-15 | ** upfront dmg なし**、`Constrict(10)` | `DamageVar(15)` 追加。Constrict は per-turn 用に維持 |
| 35 | 指折り数えて | FingerCount | 手札カウント全件コスト-1 | **コスト0** | **ドロー2**、UG ドロー3 | カウント減コスト実装。UG `EnergyCost.UpgradeBy(-1)` |
| 37 | 寝かしつけ催眠 | LullabyHypnosis | 睡眠1、**対象HP+20** | 睡眠2、**対象HP+30** | **プレイヤー** を回復 | `Heal(play.Target)` に変更 |
| 39 | 腹部への殴打 | AbdominalStrike | **16dmg+弱体2** | 20dmg+弱体2 | **8dmg、弱体なし**、UG 11dmg | DamageVar(16)、Vulnerable(2)、UG +4dmg |
| 40 | 感覚共有 | SenseShare | 単体アタック→**全体化**（このターン） | **0コスト** | Frail1+自ブロック4、UG ブロック+3 | AOE 化 Power/Patch。UG コスト0 |
| 42 | 足蹴 | Kick | 10dmg、性癖目覚め | **プレイ後山札へ** | UG で **新 Kick コピー** を山札追加 | `GetResultLocationForCardPlay` → Draw |
| 44 | 意識を飛ばす光 | GazeLight | 8blk+**脱力1** | 9blk+**脱力2** | **WeakPower** 使用 | `FrailPower` に変更 |
| 45 | 凝視 | Stare | 9dmg+トランス1 | **手札カウント+1進行** | UG **dmg+3**（12） | UG をカウント進行に差し替え |
| 47 | お仕置き | Punishment | **8×攻撃回数** dmg | **13×回数** | 固定 **9dmg+破滅5** | 被攻撃回数トラッキング×(8/13) |
| 50 | 練達 | Mastery | プレイしたカウント **すべて** UG | 1コスト | `MasteryPower` が **未UGのみ** 記録 | `IsUpgraded` ガード削除 |
| 54 | 触手の想起 | TentacleRecall | **締め付け5**（dmg なし） | 締め付け7 | **9dmg** + 締め付け5 | DamageVar/Attack 削除 |
| 55 | 糸色丁頁 | InfiniteUpgradeString | **対象** HP-15 | 喪失量+7/UG | **プレイヤー** HP-15 | `Damage(play.Target)` |
| 59 | Relax! | Relax | **脱力1** | UGなし | **WeakPower1** | `FrailPower(1)` |
| 77 | ご主人様の言うとおり | AsYouWish | Abn筋1/SM活2/Dom blk1 | Abn **筋1+敏1**/SM **活3**/blk2 | UG bonus=2 → 筋+2、活+4、敏なし | Power 側で種別ごと UG 値を分岐 |
| 79 | メンタルケア | MentalCare | 破滅除く全デバフ解除、**種類×8blk**、**沼1** | 10blk、**沼2** | 弱体/脆弱のみ、**固定blk4**、沼なし | 全デバフ解除+スケールblk+沼実装 |
| 81 | 性癖の覇者 | FetishChampion | 性癖数×**20dmg** | **25dmg** | 全目覚め+**破滅8**（dmg なし） | 性癖数×(20/25) 多段 Attack |
| 82 | ぜんぶ知ってるよ | KnowItAll | 性癖効果 **2倍** | **天賦** | **1.5倍**、UG 倍率+0.5 | Multiplier=2、UG `AddKeyword(Innate)` |
| 84 | エリクソン的誘導 | Ericksonian | 性癖刺さり→**カウント進行** | **0コスト** | **弱体1+脆弱1** 即付与 | EricksonianPower（カウント進行）。UG コスト0 |
| 87 | ゼロへの近道 | ZeroShortcut | 3→2→1→0 **blk 繰返**+手札カウント0 | **2コスト** | blk なし、手札1枚0化のみ、**UG なし** | blk ループ+UG `EnergyCost.UpgradeBy(-1)` |
| 92 | 催眠導入 | HypnosisIntro | **2枚ドロー**（逓減） | **3枚ドロー** | **トランス1+破滅4** | ドロー逓減ロジック実装 |
| 93 | 過集中 | OverFocus | 手札スキル数×**E**、廃棄 | 廃棄消滅 | **性癖スロット+1** | 手札スキル→GainEnergy、UG Remove Exhaust |
| 94 | 底なしの沼 | BottomlessBog | EOT **(トランス+沼)×破滅10** | 破滅**15** | **BogPower(3)** 付与のみ | BottomlessBogPower（EOT 破滅）。UG 15 |
| 95 | 術式の開示 | RitualReveal | **天賦**、山札からカウント2枚 | **0コスト** | ランダム性癖+破滅5 | Innate+山札サーチ。UG コスト0 |
| 96 | 鼓動の共有 | HeartbeatShare | 心臓効果を味方**1人と共有** | **全員**共有 | 味方に `StolenHeart` **付与** | 所持心臓効果の共有 Power |

---

## needs_inference 一覧（11件）

### No.4 スライム催眠（SlimeHypnosis）
- **CSV UG:** 「粘液付与**3**枚」
- **コード:** Slimed 5→**8**（+3 解釈、コメントあり）
- **推定:** CSV の「3」が絶対値か +3 差分か要設計確認。現コードは +3→8。

### No.17 カタレプシー（Catalepsy）
- **CSV BASE:** スロー付与（量の記載なし）
- **CSV UG:** トランス中スロー蓄積リセットなし
- **コード:** Slow=3、UG Slow+1 **かつ** `PersistIfTranced=true`
- **推定:** UG は persist のみか、Slow+1 も意図か。

### No.18 状態異常催眠（StatusHypnosis）
- **CSV BASE:** トランス中に状態異常・呪い playable
- **コード:** `StatusHypnosisPower` マーカーのみ（TODO）
- **推定:** BASE 未実装。UG「天賦」は `Innate` 済み。

### No.28 脳くちゅ催眠（BrainSlimeHypnosis）
- **CSV:** 攻撃リダイレクト + トランス1。UG 全員
- **コード:** リダイレクト + **ConfusedPower(1)** 追加
- **推定:** Confused は意図的拡張か CSV 漏れか。

### No.52 トランスに溶けゆく（MeltIntoTrance）
- **CSV:** 対象の「トランスに落ちた回数」×(15/20) dmg
- **コード:** `TranceFallTracker` = **累積トランススタック合計**
- **推定:** 「回数」= 付与イベント数かスタック総量か。

### No.72 壱佰捌煩悩（HundredEight）
- **CSV UG:** プレイ後 **山札** へ
- **コード:** `OnUpgrade` **なし**（廃棄のまま）
- **推定:** UG 未実装。BASE の 108hit vs 108dmg 単発も要確認。

### No.80 布教欲求（Proselytize）
- **CSV:** 推し活+沼2、戦闘終了15G
- **コード:** `OshiActivityPower` にターン終了時破滅/沼コピーあり
- **推定:** 「推し活」の詳細が CSV に未記載。UG 25G は一致。

### No.89 マシュマロ回答（MarshmallowAnswer）
- **CSV:** 呪い2枚追加；攻撃予定時 **ブロック39に上書き**+沼2
- **コード:** 呪い追加 **後に** block39+沼（両方実行）
- **推定:** 「上書き」= 呪い追加をスキップするのか。

### No.91 首輪と調教（CollarTraining）
- **CSV:** 引き寄せ；済み→破滅15+DomSub。UG コマンド2枚+…
- **コード:** 未済時 **9dmg**（CSV 記載なし）。UG コマンド2枚は概ね一致
- **推定:** 未済 dmg は StS 慣習のデフォルトか。

### No.98 フリーハグ（FreeHug）
- **CSV:** 引き寄せ；済み→破滅10+沼1+パス2枚。UG 破滅15
- **コード:** 未済時 **6dmg**。UG 破滅15 OK
- **推定:** 未済 dmg は CSV 省略の可能性。

### No.56–66 DomSub 命令トークン（Kneel/Look/Come/Relax/Present/Trance/Crawl/DontMove/Roll/Cum/Good）
- **CSV:** rarity=コモン、UGなし
- **コード:** `CardRarity.Token`、効果・Exhaust は概ね一致
- **推定:** Token は報酬プール除外の意図的メタデータ差。Relax の Weak/Frail は No.59 mismatch 参照。

---

## OK 一覧（63件）

1, 2, 3, 6, 8, 9, 10, 11, 12, 13, 14, 19, 21, 22, 23, 24, 25, 26, 27, 30, 31, 32, 34, 36, 38, 41, 43, 46, 48, 49, 51, 53, 56, 57, 58, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 78, 83, 85, 86, 88, 90, 97, 99, 100, 101, 102, 103, 104

---

## 監査方法

1. `_card_audit_report.txt` の `Class:Xxx` で No→クラスを特定
2. 各 `.cs` から `HypnosisCreatorCard(` コスト、`CanonicalVars`（DamageVar/BlockVar/DynamicVar/PowerVar）、`CanonicalKeywords`、`OnUpgrade` を抽出
3. CSV `effect` / `ug` から数値・キーワード（廃棄/保留/天賦/コスト）を正規表現抽出して比較
4. 複雑効果（AOE、意図操作、山札位置）は手動で `.cs` と Power ファイルを読んで判定
5. UG が差分のみの場合は BASE+差分または絶対値を推論（`ug_infer` フラグ）

---

## 関連ファイル

- 機械可読修正リスト: `_ug_audit_fixes.json`
- 補助スクリプト（自動抽出）: `_ug_audit_script.py`（数値中心。挙動差分は手動監査で上書き）
