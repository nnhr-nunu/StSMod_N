// Hypno Creator（StSMod_N）のフィードバックと匿名プレイデータを受け取る Worker。
// MOD に入るのはこの Worker の URL だけ。通知先の Discord Webhook は secret（DISCORD_WEBHOOK_URL）。

const FEEDBACK_MAX_CHARS = 8000;
const FEEDBACK_MAX_BYTES = 32 * 1024;
const RUN_MAX_BYTES = 256 * 1024;
const FEEDBACK_PER_HOUR = 10;
const RUNS_PER_HOUR = 60;
const DISCORD_DESCRIPTION_MAX = 4000;

const HEX64 = /^[0-9a-f]{64}$/;
const TOKEN = /^[A-Za-z0-9_.+\-]{1,64}$/;
const MODEL_ID = /^[A-Za-z0-9_.\-]{1,128}$/;
const LANGUAGE = /^([a-z]{3}|unknown)$/;
// タブ・改行以外の制御文字は受け付けない
const FORBIDDEN_CONTROL = /[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]/;

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    try {
      if (request.method === "GET" && url.pathname === "/v1/health") {
        return json({ ok: true });
      }
      if (request.method === "POST" && url.pathname === "/v1/feedback") {
        return await handleFeedback(request, env, ctx);
      }
      if (request.method === "POST" && url.pathname === "/v1/runs") {
        return await handleRun(request, env, ctx);
      }
      return json({ error: "not_found" }, 404);
    } catch (error) {
      console.error("unhandled", error);
      return json({ error: "server_error" }, 500);
    }
  },
};

async function handleFeedback(request, env, ctx) {
  const body = await readJson(request, FEEDBACK_MAX_BYTES);
  if (body.error) return body.error;

  const report = body.value;
  const message = typeof report.message === "string" ? report.message.trim() : "";
  const valid =
    report.schemaVersion === 1 &&
    isMatch(report.reportId, HEX64) &&
    isMatch(report.modVersion, TOKEN) &&
    isMatch(report.language, LANGUAGE) &&
    message.length > 0 &&
    message.length <= FEEDBACK_MAX_CHARS &&
    !FORBIDDEN_CONTROL.test(message);
  if (!valid) return json({ error: "invalid" }, 400);

  if (await isOverLimit(env, ctx, request, "feedback", FEEDBACK_PER_HOUR)) {
    return json({ error: "rate_limited" }, 429);
  }

  const result = await env.DB.prepare(
    `INSERT INTO feedback (report_id, received_at, mod_version, language, message)
     VALUES (?1, ?2, ?3, ?4, ?5)
     ON CONFLICT (report_id) DO NOTHING
     RETURNING id`,
  )
    .bind(report.reportId, new Date().toISOString(), report.modVersion, report.language, message)
    .first();

  // 同じ reportId の再送は保存済みとして受理し、通知は重ねない
  if (result && env.DISCORD_WEBHOOK_URL) {
    ctx.waitUntil(notifyDiscord(env.DISCORD_WEBHOOK_URL, result.id, report, message));
  }
  return json({ accepted: true });
}

async function handleRun(request, env, ctx) {
  const body = await readJson(request, RUN_MAX_BYTES);
  if (body.error) return body.error;

  const report = body.value;
  if (!isValidRun(report)) return json({ error: "invalid" }, 400);

  if (await isOverLimit(env, ctx, request, "runs", RUNS_PER_HOUR)) {
    return json({ error: "rate_limited" }, 429);
  }

  await env.DB.prepare(
    `INSERT INTO runs (report_id, received_at, mod_version, game_version, character, outcome,
                       ascension, player_count, floors_visited, payload)
     VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10)
     ON CONFLICT (report_id) DO NOTHING`,
  )
    .bind(
      report.reportId,
      new Date().toISOString(),
      report.modVersion,
      report.gameVersion,
      report.character,
      report.outcome,
      report.ascension,
      report.playerCount,
      report.floorsVisited,
      JSON.stringify(report),
    )
    .run();
  return json({ accepted: true });
}

function isValidRun(r) {
  if (!r || r.schemaVersion !== 1) return false;
  if (!isMatch(r.reportId, HEX64)) return false;
  if (!isMatch(r.modVersion, TOKEN) || !isMatch(r.gameVersion, TOKEN)) return false;
  if (!isMatch(r.language, LANGUAGE) || !isMatch(r.gameMode, TOKEN)) return false;
  if (!["win", "loss", "abandoned"].includes(r.outcome)) return false;
  if (!isMatch(r.character, MODEL_ID)) return false;
  if (r.killedBy != null && !isMatch(r.killedBy, MODEL_ID)) return false;
  if (!isInt(r.ascension, 0, 20) || !isInt(r.playerCount, 1, 16)) return false;
  if (!isInt(r.modifierCount, 0, 100) || !isInt(r.floorsVisited, 0, 1000)) return false;
  if (!isInt(r.actsVisited, 0, 16)) return false;
  if (r.runTimeSeconds != null && !isInt(r.runTimeSeconds, 0, 31536000)) return false;
  if (r.finalHp != null && !isInt(r.finalHp, 0, 100000)) return false;
  if (r.finalMaxHp != null && !isInt(r.finalMaxHp, 0, 100000)) return false;
  if (typeof r.hasOtherGameplayMods !== "boolean") return false;

  return (
    isArrayOf(r.partyCharacters, 16, (id) => isMatch(id, MODEL_ID)) &&
    isArrayOf(r.relics, 500, (id) => isMatch(id, MODEL_ID)) &&
    isArrayOf(
      r.deck,
      1000,
      (c) => c && isMatch(c.id, MODEL_ID) && isInt(c.upgrade, 0, 100) && isInt(c.count, 1, 1000),
    ) &&
    isArrayOf(
      r.cardChoices,
      2000,
      (c) =>
        c &&
        isInt(c.floor, 1, 1000) &&
        isMatch(c.id, MODEL_ID) &&
        isInt(c.upgrade, 0, 100) &&
        typeof c.picked === "boolean",
    ) &&
    isArrayOf(
      r.relicChoices,
      2000,
      (c) => c && isInt(c.floor, 1, 1000) && isMatch(c.id, MODEL_ID) && typeof c.picked === "boolean",
    )
  );
}

// 1時間ごとの固定枠で数える。元の IP は保存せず、枠ごとに変わるハッシュだけを使う
async function isOverLimit(env, ctx, request, kind, perHour) {
  const ip = request.headers.get("CF-Connecting-IP") ?? "unknown";
  const hour = Math.floor(Date.now() / 3_600_000);
  const key = await sha256Hex(`${kind}|${hour}|${ip}`);
  const row = await env.DB.prepare(
    `INSERT INTO rate_limits (key, window_hour, count) VALUES (?1, ?2, 1)
     ON CONFLICT (key) DO UPDATE SET count = count + 1
     RETURNING count`,
  )
    .bind(key, hour)
    .first();
  ctx.waitUntil(
    env.DB.prepare("DELETE FROM rate_limits WHERE window_hour < ?1").bind(hour - 1).run(),
  );
  return row.count > perHour;
}

async function notifyDiscord(webhookUrl, feedbackId, report, message) {
  const truncated = message.length > DISCORD_DESCRIPTION_MAX;
  const description = truncated
    ? `${message.slice(0, DISCORD_DESCRIPTION_MAX)}…\n\n（全文は D1 の feedback テーブル id=${feedbackId}）`
    : message;
  const payload = {
    username: "Hypno Creator Feedback",
    // 本文の @everyone などで通知が飛ばないようにする
    allowed_mentions: { parse: [] },
    embeds: [
      {
        title: `フィードバック #${feedbackId}`,
        description,
        color: 0x7aa7d4,
        fields: [
          { name: "MOD", value: report.modVersion, inline: true },
          { name: "言語", value: report.language, inline: true },
        ],
        timestamp: new Date().toISOString(),
      },
    ],
  };
  try {
    const response = await fetch(webhookUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) console.error("discord", response.status, await response.text());
  } catch (error) {
    console.error("discord", error);
  }
}

async function readJson(request, maxBytes) {
  const declared = Number(request.headers.get("Content-Length") ?? "0");
  if (declared > maxBytes) return { error: json({ error: "too_large" }, 413) };
  const buffer = await request.arrayBuffer();
  if (buffer.byteLength > maxBytes) return { error: json({ error: "too_large" }, 413) };
  try {
    const value = JSON.parse(new TextDecoder().decode(buffer));
    if (!value || typeof value !== "object" || Array.isArray(value)) {
      return { error: json({ error: "invalid" }, 400) };
    }
    return { value };
  } catch {
    return { error: json({ error: "invalid" }, 400) };
  }
}

function isMatch(value, pattern) {
  return typeof value === "string" && pattern.test(value);
}

function isInt(value, min, max) {
  return Number.isInteger(value) && value >= min && value <= max;
}

function isArrayOf(value, maxLength, predicate) {
  return Array.isArray(value) && value.length <= maxLength && value.every(predicate);
}

async function sha256Hex(text) {
  const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(text));
  return [...new Uint8Array(digest)].map((b) => b.toString(16).padStart(2, "0")).join("");
}

function json(body, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json; charset=utf-8" },
  });
}
