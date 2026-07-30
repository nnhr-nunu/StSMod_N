# -*- coding: utf-8 -*-
"""Refresh _csv_cards_1_104.json from Downloads CSV and report diffs."""
from __future__ import annotations

import csv
import json
from pathlib import Path

CSV_PATH = Path(r"c:\Users\homut\Downloads\ヒプノクリエイター.csv")
JSON_PATH = Path(r"D:\Dev\antigravity\StSMod_N\_csv_cards_1_104.json")

with open(CSV_PATH, "r", encoding="utf-8-sig", newline="") as f:
    reader = csv.DictReader(f)
    print("Fieldnames:", reader.fieldnames)
    rows = []
    for row in reader:
        no = (row.get("No") or "").strip()
        if not no.isdigit():
            continue
        n = int(no)
        if n < 1 or n > 104:
            continue
        rows.append(
            {
                "no": n,
                "name": (row.get("カード名称（日本語）") or "").strip(),
                "type": (row.get("種別") or "").strip(),
                "build": (row.get("ビルド") or "").strip(),
                "cost": (row.get("コスト") or "").strip(),
                "effect": (row.get("効果説明") or "").strip(),
                "ug": (row.get("UG時効果（変更点のみ記載）") or "").strip(),
                "rarity": (row.get("レア度") or "").strip(),
                "tags": (row.get("タグ") or "").strip(),
                "id": (row.get("id") or "").strip(),
                "notes": (row.get("備考") or "").strip(),
            }
        )

rows.sort(key=lambda r: r["no"])
print("Parsed cards 1-104:", len(rows))
missing = sorted(set(range(1, 105)) - {r["no"] for r in rows})
print("Missing nos:", missing)

old = json.loads(JSON_PATH.read_text(encoding="utf-8"))
old_by_no = {r["no"]: r for r in old}
diff_count = 0
for r in rows:
    o = old_by_no.get(r["no"])
    if not o:
        print("NEW", r["no"], r["name"])
        diff_count += 1
        continue
    for k in ["name", "type", "build", "cost", "effect", "ug", "rarity"]:
        if o.get(k, "") != r.get(k, ""):
            print(f"DIFF No.{r['no']} {k}:")
            print("  OLD:", repr(o.get(k, ""))[:160])
            print("  NEW:", repr(r.get(k, ""))[:160])
            diff_count += 1

print("Total field diffs:", diff_count)
JSON_PATH.write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")
print("Updated", JSON_PATH)
