# CSV ↔ 実装 セルフチェック報告（No.1–104）

- **生成日:** 2026-07-19
- **CSV:** `c:\Users\homut\Downloads\ヒプノクリエイター.csv`（UTF-8 BOM）
- **再パース結果:** `_csv_cards_1_104.json` と **差分0**（更新済み・列: No / name / type / build / cost / effect / ug / rarity / tags / id / notes）
- **マッピング:** `_card_audit_report.txt` の `No.N | Class:Foo`（104件すべて対応）
- **検証対象:** `HypnosisCreatorCode/Cards/**/{Class}.cs` ＋ `HypnosisCreator/localization/jpn/cards.json`
- **方針:** ゲームプレイ上の明確なずれのみ。文言フォーマット差は除外。旧 `_ug_audit_report.md` は現行コードで再検証（盲信なし）
- **コード変更:** なし（監査のみ）

---

## サマリー

| 区分 | 件数 | 説明 |
|------|------|------|
| **OK** | **89** | コスト／種別／レア／JPタイトル／JP説明／主要数値・UGがCSVと一致（または許容内） |
| **確定ずれ** | **12** | 性癖タグ（`CardFetishes`）の欠落または過剰が明確 |
| **意図的近似** | **3** | `task.md` 記載のスタブ（Confused / マーカー / StolenHeart） |
| **要確認** | **7** | 解釈分岐・CSV省略・メタデータ差で自動断定不可 |

### カテゴリ別（横断）

| カテゴリ | 結果 |
|----------|------|
| A. Cost / Type / Rarity | **全件一致**（※命令トークン No.56–66 の `Token` vs CSV「コモン」は要確認） |
| B. JP title vs CSV name | **全件一致** |
| C. JP description vs CSV effect | **完全一致（ドリフト0）** — 以前同期済み文言が維持されている |
| D. 数値・BASE/UG | 旧 ug_audit の大半は **現行で解消済み**。残る明確な数値ズレはなし |
| E. Fetish tags | **12件ずれ**（下記） |
| F. Count/Retain/Exhaust/Innate | 主要カードは一致。トークンの Exhaust は基底 `TrainingCommand` で付与 |
| G. 既知スタブ | 3件とも **現状も近似のまま** |

---

## 確定ずれ（番号・カード名・問題・CSV・実装）

> 判定基準: CSV `ビルド` 列の `性癖：*` と、実装の `CardFetishes` を突合。  
> `ビルド` の単独「トランス」「カウント」「心臓」は性癖タグではない（メカニクス／アーキタイプ）。

| No | カード名 | Class | 問題 | CSV | 実装 |
|----|----------|-------|------|-----|------|
| 2 | 呼吸制御 | BreathControl | 性癖タグ欠落 | `性癖：SM` | `CardFetishes` なし |
| 6 | 時止めストライク | TimeStopStrike | 性癖タグ過剰 | `性癖：SM`（ビルドの「トランス」は条件側） | `[Sm, Trance]` — **Trance が余分** |
| 11 | アブノーマル | AbnormalTransform | 性癖タグ欠落 | `性癖：アブノーマル` | `CardFetishes` なし（変換先プール判定のみ Abnormal） |
| 57 | Look! | Look | 性癖タグ欠落 | `性癖：DomSub` | なし（Kneel/Come/Crawl は DomSub あり） |
| 59 | Relax! | Relax | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 60 | Present! | Present | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 61 | Trance! | Trance | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 63 | Don't Move! | DontMove | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 64 | Roll! | Roll | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 65 | Cum! | Cum | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 66 | Good! | Good | 性癖タグ欠落 | `性癖：DomSub` | なし |
| 72 | 壱佰捌煩悩 | HundredEight | 性癖タグ過剰 | `性癖：アブノーマル, SM, DomSub`（＋心臓） | `[Sm, DomSub, Abnormal, Trance]` — **Trance が余分** |

### プレイ影響（なぜ「確定」か）

- `CardFetishes` 欠落 → 「ご主人様の言うとおり」等の性癖連動・ハイライト／刺さり判定が動かない。
- Trance 過剰 → トランス性癖への刺さり・連動が CSV 想定より多く発火しうる。

### 比較用: タグ一致している命令トークン

| No | 名称 | CardFetishes |
|----|------|--------------|
| 56 | Kneel! | DomSub |
| 58 | Come! | DomSub |
| 62 | Crawl! | DomSub |

---

## 仕様どおりだが近似実装（意図的スタブ）

`task.md` 未完了とコードコメントが一致。現行も近似のまま。

| No | カード名 | Class | CSV 意図 | 実装の近似 | 備考 |
|----|----------|-------|----------|------------|------|
| 18 | 状態異常催眠 | StatusHypnosis | トランス中の敵に状態異常・呪いをプレイ可能 | `StatusHypnosisPower` マーカーのみ（効果 TODO） | UG「天賦」は `AddKeyword(Innate)` 済み |
| 20 | 好き好き催眠 | LoveHypnosis | バフ意図の対象をプレイヤーへリダイレクト＋トランス1 | `ConfusedPower(2)` ＋トランス1 | UG「ブロック意図も含む」は未分岐（API待ち） |
| 96 | 鼓動の共有 | HeartbeatShare | 所持心臓パッシブを味方と共有 | 味方に `StolenHeart` を付与 | UG「全員」は `IsUpgraded` で味方全員へ。ソロでは実質不発 |

---

## 要確認（自動では断定できない）

| No | カード名 | Class | 論点 | CSV | 実装 | 判断のポイント |
|----|----------|-------|------|-----|------|----------------|
| 17 | カタレプシー | Catalepsy | スロー量の記載なし | 「スローを付与」 | `Slow=3`、UGで `PersistIfTranced` | ベース3が仕様か未記載か |
| 50 | 練達 | Mastery | 無限UGカードとの相互作用 | プレイしたカウントをすべてUG | 戦闘終了時 `!IsUpgraded` のみ `Upgrade` | 通常カードは妥当。糸色丁頁など `MaxUpgradeLevel` 無限は追加UGされない |
| 52 | トランスに溶けゆく | MeltIntoTrance | 「落ちた回数」の定義 | 回数×15/20 dmg | `TranceFallTracker.TotalApplied`（付与スタック累計） | イベント回数なら別カウントが必要 |
| 56–66 | 命令トークン一式 | Token/* | レア度メタデータ | CSV「コモン」 | `CardRarity.Token`（ライブラリ非表示・プール除外） | 意図的ならOK。報酬出現の差のみ |
| 89 | マシュマロ回答 | MarshmallowAnswer | 「上書き」の意味 | 攻撃予定時ブロック39に上書き＋沼2 | 呪い追加の**後**、攻撃意図がいれば `GainBlock(39)`＋沼 | (1) 呪いをスキップする分岐か (2) ブロックを「加算」ではなく「値を39にセット」か |
| 91 | 首輪と調教 | CollarTraining | CSVにないダメージ | 引き寄せ／済み時破滅15＋DomSub | 常時 **9ダメージ** 後に分岐 | Attack 体裁のデフォルトか、CSV省略か |
| 98 | フリーハグ&lt;3 | FreeHug | CSVにないダメージ | 引き寄せ／済み時破滅10＋沼1＋パス | 常時 **6ダメージ** 後に分岐 | 同上 |

---

## 旧 ug_audit で「mismatch」だったが現行では解消済み（参考）

以下は `_ug_audit_report.md` 時点の指摘だが、**現行コードでは CSV と整合**を確認した（再発防止用メモ）。

| No | 名称 | 確認結果（現行） |
|----|------|------------------|
| 5 | 心停止催眠 | UG時 `GrantBonusRelic` → 停止時 `StolenHeart` |
| 7 | 感度3000倍 | `SensitivityPower` が ×3000 |
| 15 | 前戯 | Vulnerable1＋Weak1、UG各+1 |
| 16 | ラポール | プレイ時カウント進行＋UGで無条件進行 |
| 29 | ちゃんとできるね？ | Weak1＋Bog1、UG Weak2 |
| 33 | 植物寄生催眠 | 15dmg＋Constrict10、UG+5、Attack/コスト3/Uncommon |
| 35 | 指折り数えて | `AdvanceHandCountCards`＝手札カウントのコスト−1 |
| 37 | 寝かしつけ催眠 | 対象 Heal、睡眠UG |
| 39 | 腹部への殴打 | 16dmg＋Vulnerable2、UG+4 |
| 40 | 感覚共有 | SenseSharePower（全体化）、UGコスト0 |
| 42 | 足蹴 | UGで `GetResultLocation` → 山札 |
| 44 | 意識を飛ばす光 | 脱力＝`WeakPower`（正） |
| 45 | 凝視 | UG時カウント進行 |
| 47 | お仕置き | 被攻撃回数×8、UG×13 |
| 54 | 触手の想起 | Constrictのみ（dmgなし） |
| 55 | 糸色丁頁 | 対象 LoseHp |
| 59 | Relax! | 脱力＝Weak（タグ欠落のみ残存） |
| 72 | 壱佰捌煩悩 | UGで山札へ（Tranceタグ過剰のみ残存） |
| 77–96 他 | AsYouWish / MentalCare / FetishChampion / KnowItAll / Ericksonian / ZeroShortcut / HypnosisIntro / OverFocus / BottomlessBog / RitualReveal 等 | 数値・UGキーワードは概ね一致 |

---

## 監査手順（再現用）

1. Downloads CSV を UTF-8-SIG で再パース → `_csv_cards_1_104.json`（今回差分0）
2. `_card_audit_report.txt` から No→Class を取得
3. 各 `.cs` の ctor（cost/type/rarity）・`CanonicalVars`・`OnUpgrade`・`CardFetishes`・キーワードを読取
4. `jpn/cards.json` の title/description を CSV name/effect と正規化比較
5. 旧 ug_audit の mismatch 候補を個別に現行コードで再確認
6. `task.md` スタブ3件をコードコメントと突合

補助スクリプト（作業用・ゲーム非改変）: `_audit_refresh_csv.py` / `_selfcheck_audit.py` / `_selfcheck_deep.py` / `_selfcheck_final.py`

---

## 推奨フォローアップ（実装は別タスク）

1. **確定ずれ12件の `CardFetishes` 修正**（欠落付与／Trance 過剰削除）— 影響は性癖連動全般
2. 命令トークンで `ResolveFetishOnTarget` を呼ぶか方針統一（現状 Knife 以外ほぼ未呼び出し）
3. スタブ3件は API／仕様待ちのまま維持でよい（task.md 継続）
4. 要確認7件はデザイン確認後に CSV か実装のどちらかを正とする
