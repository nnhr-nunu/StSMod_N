# -*- coding: utf-8 -*-
"""Final focused audit: fetishes, desc drift, remaining gameplay mismatches."""
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
FETISH_MAP = {
    "SM": "Sm",
    "DomSub": "DomSub",
    "アブノーマル": "Abnormal",
    "トランス": "Trance",
}


def entry_for(cls: str) -> str:
    if cls in ALIASES:
        return ALIASES[cls]
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", cls)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).upper()


def find(cls: str) -> Path | None:
    hits = list(CARDS.rglob(f"{cls}.cs"))
    return hits[0] if hits else None


def csv_fetishes(build: str) -> set[str]:
    out: set[str] = set()
    for m in re.finditer(r"性癖：\s*([^\s,，]+)", build or ""):
        key = m.group(1).strip()
        if key in FETISH_MAP:
            out.add(FETISH_MAP[key])
    return out


def card_fetishes(text: str) -> set[str]:
    fm = re.search(r"CardFetishes\s*=>\s*(\[[^\]]*\]|new[^\n;]+)", text, re.S)
    if not fm:
        return set()
    return set(re.findall(r"FetishType\.(\w+)", fm.group(1)))


def normalize(s: str) -> str:
    return re.sub(r"\s+", "", (s or "").replace("\r\n", "\n").replace("\r", "\n"))


fetish_miss = []
desc_drift = []
for no in range(1, 105):
    c = CSV[no]
    cls = MAP[no]
    p = find(cls)
    t = p.read_text(encoding="utf-8") if p else ""
    want = csv_fetishes(c.get("build", ""))
    have = card_fetishes(t)
    if want != have:
        fetish_miss.append(
            (no, c["name"], cls, sorted(want), sorted(have), c.get("build", ""))
        )
    entry = entry_for(cls)
    desc = JPN.get(f"HYPNOSISCREATOR-{entry}.description", "")
    if normalize(desc) != normalize(c["effect"]):
        desc_drift.append((no, c["name"], cls, c["effect"][:60], desc[:60]))

print(f"Fetish mismatches: {len(fetish_miss)}")
for row in fetish_miss:
    print(f"  No.{row[0]:3d} {row[1]} ({row[2]}): csv={row[3]} code={row[4]} build={row[5]!r}")

print(f"\nJP desc drift (exact): {len(desc_drift)}")
for row in desc_drift[:30]:
    print(f"  No.{row[0]:3d} {row[1]}: csv={row[3]!r} loc={row[4]!r}")
if len(desc_drift) > 30:
    print(f"  ... +{len(desc_drift)-30} more")

# Manual spot checks for known complex cards
print("\n=== SPOT CHECKS ===")
spots = {
    2: "BreathControl fetish",
    4: "SlimeHypnosis UG Slimed",
    5: "CardiacArrest UG relic",
    7: "Sensitivity 3000",
    11: "AbnormalTransform fetish",
    28: "BrainSlime Confused?",
    35: "FingerCount = AdvanceHandCount (=cost-1) OK",
    42: "Kick UG draw pile",
    44: "GazeLight Weak=脱力",
    45: "Stare UG advance count",
    52: "MeltIntoTrance tracker",
    59: "Relax Weak",
    72: "HundredEight UG draw + Trance fetish extra",
    87: "ZeroShortcut block 3..0 + BlockVar6 display",
    91: "CollarTraining extra dmg",
    98: "FreeHug extra dmg",
}

for no, note in spots.items():
    cls = MAP[no]
    p = find(cls)
    t = p.read_text(encoding="utf-8")
    print(f"\nNo.{no} {CSV[no]['name']} — {note}")
    if "Confused" in t:
        print("  has Confused")
    if "StolenHeart" in t:
        print("  has StolenHeart")
    if "FrailPower" in t:
        print("  has FrailPower")
    if "WeakPower" in t:
        print("  has WeakPower")
    slim = re.search(r'Slimed["\']?\s*,\s*(\d+)', t) or re.search(
        r'DynamicVar\("Slimed",\s*(\d+)', t
    )
    if slim:
        print(f"  Slimed base={slim.group(1)}")
    ug = re.search(r"UpgradeValueBy\((-?\d+)", t)
    if ug and no == 4:
        print(f"  UG delta={ug.group(1)}")
    dmg = re.search(r"DamageVar\((\d+)", t)
    if dmg:
        print(f"  DamageVar={dmg.group(1)}")
    if "GetResultLocationForCardPlay" in t:
        print("  has GetResultLocationForCardPlay")
        if "IsUpgraded" in t and "Draw" in t:
            print("  UG->Draw present")

# TranceFallTracker semantics
tf = list((ROOT / "HypnosisCreatorCode").rglob("TranceFallTracker.cs"))
if tf:
    print("\nTranceFallTracker:")
    print(tf[0].read_text(encoding="utf-8")[:800])

# BrainSlime full
bs = find("BrainSlimeHypnosis")
if bs:
    print("\nBrainSlimeHypnosis full OnPlay mentions:")
    t = bs.read_text(encoding="utf-8")
    for line in t.splitlines():
        if any(x in line for x in ["Confused", "Redirect", "Trance", "IsUpgraded"]):
            print(" ", line.strip())
