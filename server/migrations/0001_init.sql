-- フィードバック本文（IP・Steam ID は保存しない）
CREATE TABLE feedback (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  report_id TEXT NOT NULL UNIQUE,
  received_at TEXT NOT NULL,
  mod_version TEXT NOT NULL,
  language TEXT NOT NULL,
  message TEXT NOT NULL
);

-- ラン終了時の匿名プレイデータ。集計用の列＋元の JSON（payload）
CREATE TABLE runs (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  report_id TEXT NOT NULL UNIQUE,
  received_at TEXT NOT NULL,
  mod_version TEXT NOT NULL,
  game_version TEXT NOT NULL,
  character TEXT NOT NULL,
  outcome TEXT NOT NULL,
  ascension INTEGER NOT NULL,
  player_count INTEGER NOT NULL,
  floors_visited INTEGER NOT NULL,
  payload TEXT NOT NULL
);
CREATE INDEX runs_mod_version ON runs (mod_version);

-- 連投対策。key は「種類・時間枠・IP」のハッシュで、元の IP は残らない。2時間で消す
CREATE TABLE rate_limits (
  key TEXT PRIMARY KEY,
  window_hour INTEGER NOT NULL,
  count INTEGER NOT NULL
);
CREATE INDEX rate_limits_window ON rate_limits (window_hour);
