# -*- coding: utf-8 -*-
"""Deep verify candidate mismatches against current code + jpn loc."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
CSV = {c["no"]: c for c in json.loads((ROOT / "_csv_cards_1_104.json").read_text(encoding="utf-8"))}
AUDIT = (ROOT / "_card_audit_report.txt").read_text(encoding="utf-8")
MAP = {
    int(m.group(1)): m.group(2)
    for m in re.finditer(r"No\.\s*(\d+)\s*\|\s*CSV:.*?\|\s*Class:(\w+)\s*\(", AUDIT)
}
JPN = json.loads(
    (ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json").read_text(
        encoding="utf-8"
    )
)
CARDS = ROOT / "HypnosisCreatorCode" / "Cards"

ALIASES = {
    "Sensitivity3000": "SENSITIVITY3000",
    "HcDefend": "HC_DEFEND",
    "SayYoureSorry": "SAY_YOURE_SORRY",
    "DontMove": "DONT_MOVE",
    "MetronomeCard": "METRONOME_CARD",
    "TrainingCommandCard": "TRAINING_COMMAND_CARD",
}


def entry_for(cls: str) -> str:
    if cls in ALIASES:
        return ALIASES[cls]
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", cls)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).upper()


def find(cls: str) -> Path | None:
    hits = list(CARDS.rglob(f"{cls}.cs"))
    return hits[0] if hits else None


NOS = [
    2,
    5,
    7,
    11,
    15,
    16,
    17,
    18,
    20,
    28,
    29,
    33,
    35,
    37,
    39,
    40,
    42,
    44,
    45,
    47,
    50,
    52,
    54,
    55,
    56,
    57,
    58,
    59,
    60,
    61,
    62,
    63,
    64,
    65,
    66,
    71,
    72,
    77,
    79,
    80,
    81,
    82,
    84,
    87,
    89,
    91,
    92,
    93,
    94,
    95,
    96,
    98,
]

# Also check ALL cards for cost/type/rarity/title after recent sync
RARITY = {
    "スターター": "Basic",
    "コモン": "Common",
    "アンコモン": "Uncommon",
    "レア": "Rare",
}
TYPE = {"スキル": "Skill", "アタック": "Attack", "パワー": "Power"}
TOKENS = {
    "Kneel",
    "Look",
    "Come",
    "Relax",
    "Present",
    "Trance",
    "Crawl",
    "DontMove",
    "Roll",
    "Cum",
    "Good",
}

print("=" * 80)
print("A/B: cost/type/rarity/title for ALL 1-104")
print("=" * 80)
atr_issues = []
for no in range(1, 105):
    c = CSV[no]
    cls = MAP[no]
    p = find(cls)
    t = p.read_text(encoding="utf-8") if p else ""
    m = re.search(
        r"HypnosisCreatorCard\s*\(\s*(-?\d+|CardCost\.X)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)",
        t,
    )
    if cls in TOKENS:
        # TrainingCommand base
        cost, typ, rar = "0", "Skill/varies", "Token"
        if m:
            cost, typ, rar = m.group(1), m.group(2), m.group(3)
        expect_rar = "Token"
        if rar != expect_rar:
            atr_issues.append(f"No.{no} {cls}: rarity code={rar} csv=コモン(Token意図?)")
        continue
    if not m:
        atr_issues.append(f"No.{no} {cls}: ctor not found")
        continue
    cost, typ, rar = m.group(1), m.group(2), m.group(3)
    if "X" in cost or cost == "-1":
        cost_n = "X"
    else:
        cost_n = cost
    er = RARITY.get(c["rarity"])
    et = TYPE.get(c["type"])
    ec = "X" if str(c["cost"]).upper() == "X" else str(c["cost"])
    title = JPN.get(f"HYPNOSISCREATOR-{entry_for(cls)}.title")
    local = []
    if cost_n != ec:
        local.append(f"COST csv={ec} code={cost_n}")
    if typ != et:
        local.append(f"TYPE csv={et} code={typ}")
    if rar != er:
        local.append(f"RARITY csv={er} code={rar}")
    if title != c["name"]:
        local.append(f"TITLE csv={c['name']!r} loc={title!r}")
    if local:
        atr_issues.append(f"No.{no:3d} {c['name']} ({cls}): " + "; ".join(local))

print(f"ATR issues: {len(atr_issues)}")
for x in atr_issues:
    print(x)

print("\n" + "=" * 80)
print("CANDIDATE DEEP DUMP")
print("=" * 80)

for no in NOS:
    c = CSV[no]
    cls = MAP[no]
    p = find(cls)
    t = p.read_text(encoding="utf-8") if p else ""
    entry = entry_for(cls)
    title = JPN.get(f"HYPNOSISCREATOR-{entry}.title")
    desc = JPN.get(f"HYPNOSISCREATOR-{entry}.description", "")
    print(f"\n### No.{no} {c['name']} / {cls}")
    print(f"CSV: type={c['type']} cost={c['cost']} rarity={c['rarity']} build={c['build']}")
    print(f"EFF: {c['effect']}")
    print(f"UG:  {c['ug']}")
    print(f"LOC title={title!r}")
    print(f"LOC desc={desc[:200]!r}")
    if not p:
        print("CODE: MISSING")
        continue
    # Show CanonicalVars, OnUpgrade, key lines
    for label, pat in [
        ("CTOR", r"HypnosisCreatorCard\s*\([^)]+\)"),
        ("FET", r"CardFetishes\s*=>[^;]+;"),
        ("KW", r"CanonicalKeywords\s*=>[^;]+;"),
        ("VARS", r"CanonicalVars\s*=>\s*\[[^\]]+\]"),
    ]:
        m = re.search(pat, t, re.S)
        if m:
            print(f"{label}: {' '.join(m.group(0).split())[:220]}")
    ou = re.search(
        r"protected\s+override\s+void\s+OnUpgrade\s*\(\s*\)\s*(?:=>\s*[^;]+;|\{[\s\S]*?\n\s*\})",
        t,
    )
    if ou:
        print(f"UGCODE: {' '.join(ou.group(0).split())[:220]}")
    else:
        print("UGCODE: (none)")
    # OnPlay condensed
    op = re.search(
        r"protected\s+override\s+async Task\s+OnPlay[\s\S]*?\n    \}", t
    )
    if op:
        body = " ".join(op.group(0).split())
        print(f"ONPLAY: {body[:400]}")
