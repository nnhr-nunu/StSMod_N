# -*- coding: utf-8 -*-
import csv
import json
from pathlib import Path

src = Path(r"c:\Users\homut\Downloads\ヒプノクリエイター_2026-07-21.csv")
text = src.read_text(encoding="utf-8-sig")
rows = list(csv.DictReader(text.splitlines()))
print("cols:", list(rows[0].keys()) if rows else None)
print("total rows:", len(rows))

sel = []
for r in rows:
    try:
        n = int(str(r.get("No", "")).strip())
    except ValueError:
        continue
    if 111 <= n <= 187:
        sel.append(r)

out = Path("_hearts_csv_111_187.json")
out.write_text(json.dumps(sel, ensure_ascii=False, indent=2), encoding="utf-8")
print("selected:", len(sel), "->", out)

for r in sel:
    name = r.get("カード名称（日本語）", "")
    rare = r.get("レア度", "")
    eff = (r.get("通常効果", "") or "").replace("\n", " / ")
    note = (r.get("備考", "") or "").replace("\n", " / ")
    print(f"--- {r['No']} {name} [{rare}]")
    print("EFF:", eff[:250])
    if note.strip():
        print("NOTE:", note[:400])
