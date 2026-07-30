# -*- coding: utf-8 -*-
"""Re-audit rarity/cost/type/name vs CSV after sync."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
CSV = json.loads((ROOT / "_csv_cards_1_104.json").read_text(encoding="utf-8"))
JPN = json.loads((ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json").read_text(encoding="utf-8"))
AUDIT = (ROOT / "_card_audit_report.txt").read_text(encoding="utf-8")
CARDS = ROOT / "HypnosisCreatorCode" / "Cards"

RARITY = {"スターター": "Basic", "コモン": "Common", "アンコモン": "Uncommon", "レア": "Rare"}
TYPE = {"スキル": "Skill", "アタック": "Attack", "パワー": "Power"}
TOKENS = {
    "Kneel", "Look", "Come", "Relax", "Present", "Trance",
    "Crawl", "DontMove", "Roll", "Cum", "Good",
}

mapping = {
    int(m.group(1)): m.group(2)
    for m in re.finditer(r"No\.\s*(\d+)\s*\|\s*CSV:.*?\|\s*Class:(\w+)\s*\(", AUDIT)
}


def pascal_to_snake(name: str) -> str:
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).upper()


ALIASES = {
    "Sensitivity3000": "SENSITIVITY3000",
    "HcDefend": "HC_DEFEND",
    "SayYoureSorry": "SAY_YOURE_SORRY",
    "DontMove": "DONT_MOVE",
    "MetronomeCard": "METRONOME_CARD",
    "TrainingCommandCard": "TRAINING_COMMAND_CARD",
}


def entry_for(cls: str) -> str:
    return ALIASES.get(cls, pascal_to_snake(cls))


def parse_ctor(path: Path) -> tuple[str | None, str | None, str | None]:
    text = path.read_text(encoding="utf-8")
    m = re.search(
        r"HypnosisCreatorCard\s*\(\s*(-?\d+|X)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)",
        text,
    )
    if m:
        return m.group(1), m.group(2), m.group(3)
    if "TrainingCommand(" in text:
        return "0", "Skill", "Token"
    return None, None, None


issues = []
ok = 0
for row in CSV:
    no = row["no"]
    cls = mapping.get(no)
    if not cls:
        issues.append(f"{no}: no mapping")
        continue
    files = list(CARDS.rglob(f"{cls}.cs"))
    if not files:
        issues.append(f"{no} {cls}: missing file")
        continue
    cost, typ, rar = parse_ctor(files[0])
    entry = entry_for(cls)
    title = JPN.get(f"HYPNOSISCREATOR-{entry}.title")
    desc = JPN.get(f"HYPNOSISCREATOR-{entry}.description")

    local = []
    if title != row["name"]:
        local.append(f"NAME code/loc={title!r} csv={row['name']!r}")
    if desc != row["effect"]:
        local.append("DESC")
    expect_rar = "Token" if cls in TOKENS else RARITY.get(row["rarity"])
    expect_type = TYPE.get(row["type"])
    expect_cost = "-1" if str(row["cost"]).upper() == "X" else str(row["cost"]).strip()
    if rar != expect_rar:
        local.append(f"RARITY code={rar} csv={expect_rar}")
    if typ != expect_type and cls not in TOKENS:
        # tokens may still be Skill via base
        local.append(f"TYPE code={typ} csv={expect_type}")
    if cost != expect_cost and cls not in TOKENS:
        local.append(f"COST code={cost} csv={expect_cost}")
    if local:
        issues.append(f"{no:3d} {cls}: " + "; ".join(local))
    else:
        ok += 1

out = ROOT / "_reaudit.txt"
out.write_text(
    f"OK={ok}\nISSUES={len(issues)}\n\n" + "\n".join(issues) + "\n",
    encoding="utf-8",
)
print(f"OK={ok} ISSUES={len(issues)} -> {out}")
