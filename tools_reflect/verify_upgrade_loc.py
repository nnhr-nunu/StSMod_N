# -*- coding: utf-8 -*-
"""Verify upgradeReplaceFrom substrings exist in card descriptions."""
import json
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
LANGS = ["jpn", "eng", "kor", "deu", "spa", "rus", "zhs", "zht"]
issues = []

for lang in LANGS:
    data = json.loads(
        (ROOT / "HypnosisCreator/localization" / lang / "cards.json").read_text(encoding="utf-8")
    )
    for key, val in list(data.items()):
        if not key.endswith(".upgradeReplaceFrom"):
            continue
        entry = key[: -len(".upgradeReplaceFrom")]
        desc = data.get(f"{entry}.description", "")
        if not val:
            continue
        if val not in desc:
            issues.append(f"[{lang}] {entry}: from not in description\n  from={val[:80]!r}")

out = ROOT / "_verify_upgrade_out.txt"
out.write_text(f"issues: {len(issues)}\n" + "\n".join(issues), encoding="utf-8")
print(f"wrote {len(issues)} issues to {out}")
