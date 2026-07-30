# -*- coding: utf-8 -*-
import csv, json
from pathlib import Path

path = Path(r"c:/Users/homut/Downloads/ヒプノクリエイター.csv")
out = []
with path.open(encoding="utf-8-sig", newline="") as f:
    for row in csv.DictReader(f):
        try:
            n = int(row["No"])
        except Exception:
            continue
        if 1 <= n <= 104:
            out.append({
                "no": n,
                "name": row["カード名称（日本語）"],
                "type": row["種別"],
                "cost": row["コスト"],
                "effect": row["効果説明"],
                "ug": row["UG時効果（変更点のみ記載）"],
                "rarity": row["レア度"],
                "build": row.get("ビルド", ""),
            })

Path("_csv_cards_1_104.json").write_text(
    json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8"
)
print(f"wrote {len(out)} cards")
# highlight X cost and name mismatches sample
for r in out:
    if str(r["cost"]).upper() == "X" or r["no"] in (41,42,44,45,46,47,48,49,71,78,79):
        print(f"{r['no']:3d} {r['name']} | {r['rarity']} {r['cost']} {r['type']} | UG={r['ug']!r}")
        print(f"     {r['effect'][:80]}")
