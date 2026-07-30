# -*- coding: utf-8 -*-
"""CSV UG と jpn cards.json の :diff() 連動漏れを洗い出す。"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent
loc = json.loads((ROOT / "HypnosisCreator/localization/jpn/cards.json").read_text(encoding="utf-8"))
csv_rows = json.loads((ROOT / "_csv_cards_latest.json").read_text(encoding="utf-8"))

titles: dict[str, str] = {}
for k, v in loc.items():
    if k.endswith(".title"):
        titles[v] = k[: -len(".title")]

skip_ug = {"", "なし", "—", "-", "無し", "無し。", "なし。"}

hard: list[tuple] = []
kw_only: list[tuple] = []
ok: list[tuple] = []
missing_loc: list[tuple] = []

for r in csv_rows:
    ug = (r.get("UG時効果（変更点のみ記載）") or "").strip()
    if ug in skip_ug:
        continue
    name = r["カード名称（日本語）"]
    key = titles.get(name)
    if not key:
        missing_loc.append((r["No"], name, ug[:50]))
        continue
    desc = loc.get(key + ".description", "")
    has_diff = ":diff()" in desc or ":energyDiff()" in desc
    nums = re.findall(r"(?<![A-Za-z{:_])\d+", desc)
    kw_markers = ("コスト", "廃棄", "保留", "天賦", "アップグレード", "消える", "消滅", "固有")
    is_kw = any(m in ug for m in kw_markers) and not re.search(r"\d", ug)

    if has_diff:
        ok.append((r["No"], name, ug[:40]))
    elif nums and not is_kw:
        hard.append((r["No"], name, ug, desc, key))
    elif not has_diff:
        # UG があるのに diff も数字もない、またはキーワードのみだが文言が変わらない
        kw_only.append((r["No"], name, ug, desc, key))

print("=== OK (:diff present) ===", len(ok))
print("=== HARDCODED nums, no :diff (need fix) ===", len(hard))
for no, name, ug, desc, key in hard:
    print(f"{no:>3} {name}")
    print(f"    UG: {ug}")
    print(f"    DESC: {desc}")
    print(f"    KEY: {key}")
print("=== KW/text UG without :diff (review) ===", len(kw_only))
for no, name, ug, desc, key in kw_only:
    print(f"{no:>3} {name}")
    print(f"    UG: {ug}")
    print(f"    DESC: {desc}")
print("=== missing title in loc ===", len(missing_loc))
for no, name, ug in missing_loc:
    print(f"{no:>3} {name} | {ug}")
