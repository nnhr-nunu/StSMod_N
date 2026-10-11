# stsmod-n（フィードバック・匿名プレイデータの受け口）

Hypno Creator の Mod設定画面から送られる「フィードバック」と、ラン終了時の「匿名プレイデータ」を受け取る Cloudflare Worker。

- URL: `https://stsmod-n.nnhr.workers.dev`（MOD 側は `HypnosisCreatorCode/Telemetry/TelemetryClient.cs` の `BaseUrl`）
- アカウント: nnhr（`b1921f40cc673e291e3348515ab8fca4`）
- 保存先: D1 `stsmod-n`（テーブル `feedback` / `runs` / `rate_limits`）
- フィードバックは D1 に保存し、secret `DISCORD_WEBHOOK_URL` があれば Discord にも通知する

## エンドポイント

| メソッド | パス | 内容 |
| -------- | ---- | ---- |
| GET | `/v1/health` | 動作確認（`{"ok":true}`） |
| POST | `/v1/feedback` | 本文・MOD バージョン・表示言語（1時間に10件まで / IP） |
| POST | `/v1/runs` | ラン1件の匿名データ（1時間に60件まで / IP） |

応答: 200 受理 / 400・413 内容が不正（再送しない）/ 429 回数超過 / 500 サーバー側の失敗。
項目を変えるときは `src/index.js` の `isValidRun` と MOD 側の `RunReport`（`schemaVersion`）を合わせる。

## 個人情報の扱い

- Steam ID・名前・シード・IP は保存しない
- 連投対策の `rate_limits` は「種類・1時間の枠・IP」の SHA-256 だけを持ち、2時間で消える
- Discord 通知では `allowed_mentions` を空にしているので、本文の `@everyone` などで通知は飛ばない

## デプロイ・設定（`server/` で実行）

```bash
npx wrangler@4.144.0 deploy
```

スキーマを変えたら `migrations/` に番号付きの SQL を足して:

```bash
npx wrangler@4.144.0 d1 migrations apply stsmod-n --remote
```

Discord の Webhook URL の登録・差し替え（貼り付けを求められる。リポジトリには書かない）:

```bash
npx wrangler@4.144.0 secret put DISCORD_WEBHOOK_URL
```

ローカルで試す: `npx wrangler@4.144.0 d1 migrations apply stsmod-n --local` → `npx wrangler@4.144.0 dev --local`

## データを見る

ダッシュボードの D1 → `stsmod-n` → Console に貼るか、`npx wrangler@4.144.0 d1 execute stsmod-n --remote --command "<SQL>"`。

最近のフィードバック:

```sql
SELECT id, received_at, mod_version, language, message FROM feedback ORDER BY id DESC LIMIT 20;
```

アセンション別の勝率:

```sql
SELECT ascension, COUNT(*) AS runs, SUM(outcome = 'win') AS wins,
       ROUND(100.0 * SUM(outcome = 'win') / COUNT(*), 1) AS win_rate
FROM runs GROUP BY ascension ORDER BY ascension;
```

カードのピック率（報酬に出た回数と取った回数）:

```sql
SELECT json_extract(c.value, '$.id') AS card, COUNT(*) AS offered,
       SUM(json_extract(c.value, '$.picked')) AS picked,
       ROUND(100.0 * SUM(json_extract(c.value, '$.picked')) / COUNT(*), 1) AS pick_rate
FROM runs, json_each(runs.payload, '$.cardChoices') AS c
GROUP BY card ORDER BY offered DESC;
```

最終デッキに入っていたランの勝率（他のゲームプレイ MOD を入れていないランだけ）:

```sql
SELECT card, COUNT(*) AS runs, ROUND(100.0 * AVG(win), 1) AS win_rate
FROM (
  SELECT DISTINCT runs.id, json_extract(d.value, '$.id') AS card, runs.outcome = 'win' AS win
  FROM runs, json_each(runs.payload, '$.deck') AS d
  WHERE json_extract(runs.payload, '$.hasOtherGameplayMods') = 0
)
GROUP BY card ORDER BY runs DESC;
```

デプロイ確認で入れたテスト行（2026-10-12）を消す:

```sql
DELETE FROM runs WHERE json_extract(payload, '$.killedBy') = 'ENCOUNTER.TEST';
```
