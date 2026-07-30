# -*- coding: utf-8 -*-
import json
from pathlib import Path

rows = json.loads(Path("_csv_cards_1_104.json").read_text(encoding="utf-8"))
out = []
for r in rows:
    if 56 <= r["no"] <= 66 or r["no"] in (9, 44, 49, 51, 70, 71):
        out.append(r)
Path("_token_csv.json").write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
print("ok", len(out))
