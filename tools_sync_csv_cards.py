# -*- coding: utf-8 -*-
"""Sync card rarity/cost/type/title/description from CSV using audit No→Class map."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
CSV_JSON = ROOT / "_csv_cards_1_104.json"
AUDIT = ROOT / "_card_audit_report.txt"
JPN_CARDS = ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json"
CARDS_DIR = ROOT / "HypnosisCreatorCode" / "Cards"

RARITY_MAP = {
    "スターター": "Basic",
    "コモン": "Common",
    "アンコモン": "Uncommon",
    "レア": "Rare",
}
TYPE_MAP = {
    "スキル": "Skill",
    "アタック": "Attack",
    "パワー": "Power",
}


def pascal_to_snake(name: str) -> str:
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).upper()


def load_mapping() -> dict[int, str]:
    text = AUDIT.read_text(encoding="utf-8")
    pat = re.compile(r"No\.\s*(\d+)\s*\|\s*CSV:.*?\|\s*Class:(\w+)\s*\(")
    return {int(m.group(1)): m.group(2) for m in pat.finditer(text)}


def find_class_file(class_name: str) -> Path | None:
    matches = list(CARDS_DIR.rglob(f"{class_name}.cs"))
    return matches[0] if matches else None


def resolve_entry(class_name: str, jpn: dict) -> str | None:
    snake = pascal_to_snake(class_name)
    # Special cases that don't match simple conversion
    aliases = {
        "Sensitivity3000": "SENSITIVITY3000",
        "HcDefend": "HC_DEFEND",
        "SayYoureSorry": "SAY_YOURE_SORRY",
        "DontMove": "DONT_MOVE",
        "MetronomeCard": "METRONOME_CARD",
        "TrainingCommandCard": "TRAINING_COMMAND_CARD",
    }
    entry = aliases.get(class_name, snake)
    key = f"HYPNOSISCREATOR-{entry}.title"
    if key in jpn:
        return entry
    # fallback: search by class-derived prefixes
    for k in jpn:
        if k.startswith("HYPNOSISCREATOR-") and k.endswith(".title"):
            e = k[len("HYPNOSISCREATOR-") : -len(".title")]
            if e.replace("_", "") == class_name.upper():
                return e
    return entry if key else None


def patch_csharp(path: Path, rarity: str | None, cost: int | str | None, card_type: str | None) -> bool:
    text = path.read_text(encoding="utf-8")
    original = text

    # Skip TrainingCommand subclasses (token base sets cost/rarity)
    if "TrainingCommand(" in text and "HypnosisCreatorCard(" not in text:
        return False

    cost_pat = r"(-?\d+|X)"
    pattern = re.compile(
        rf"(HypnosisCreatorCard\s*\(\s*){cost_pat}(\s*,\s*CardType\.)(\w+)(\s*,\s*CardRarity\.)(\w+)",
        re.MULTILINE,
    )

    def repl(m: re.Match) -> str:
        new_cost = m.group(2)
        new_type = m.group(4)
        new_rar = m.group(6)
        if cost is not None:
            new_cost = str(cost)
        if card_type:
            new_type = card_type
        if rarity:
            new_rar = rarity
        return f"{m.group(1)}{new_cost}{m.group(3)}{new_type}{m.group(5)}{new_rar}"

    text2, n = pattern.subn(repl, text, count=1)
    if n == 0:
        pattern2 = re.compile(
            rf"(HypnosisCreatorCard\s*\(\s*){cost_pat}(\s*,\s*\n\s*CardType\.)(\w+)(\s*,\s*CardRarity\.)(\w+)",
            re.MULTILINE,
        )
        text2, n = pattern2.subn(repl, text, count=1)

    if n == 0:
        print(f"  WARN: constructor not patched: {path.name}")
        return False

    if text2 != original:
        path.write_text(text2, encoding="utf-8")
        return True
    return False


def main() -> None:
    rows = {r["no"]: r for r in json.loads(CSV_JSON.read_text(encoding="utf-8"))}
    mapping = load_mapping()
    jpn = json.loads(JPN_CARDS.read_text(encoding="utf-8"))

    loc_changed = 0
    cs_changed = 0
    missing = []

    for no, class_name in sorted(mapping.items()):
        row = rows.get(no)
        if not row:
            missing.append(f"no csv {no}")
            continue

        entry = resolve_entry(class_name, jpn)
        if not entry:
            missing.append(f"no loc entry {class_name}")
            continue

        title_key = f"HYPNOSISCREATOR-{entry}.title"
        desc_key = f"HYPNOSISCREATOR-{entry}.description"
        if title_key not in jpn:
            missing.append(f"missing key {title_key}")
            continue

        name = row["name"]
        effect = row["effect"]
        if jpn.get(title_key) != name or jpn.get(desc_key) != effect:
            jpn[title_key] = name
            jpn[desc_key] = effect
            loc_changed += 1

        rar_csv = row["rarity"]
        type_csv = row["type"]
        cost_csv = str(row["cost"]).strip()

        # Tokens keep CardRarity.Token in code
        if class_name in {
            "Kneel", "Look", "Come", "Relax", "Present", "Trance",
            "Crawl", "DontMove", "Roll", "Cum", "Good",
        }:
            rarity = "Token"
        else:
            rarity = RARITY_MAP.get(rar_csv)

        card_type = TYPE_MAP.get(type_csv)
        if cost_csv.upper() == "X":
            cost: int | str | None = -1  # StS X-cost convention; verify compile
        else:
            try:
                cost = int(cost_csv)
            except ValueError:
                cost = None

        path = find_class_file(class_name)
        if path is None:
            missing.append(f"missing file {class_name}")
            continue

        if patch_csharp(path, rarity, cost, card_type):
            cs_changed += 1
            print(f"patched {no:3d} {class_name}: rar={rarity} cost={cost} type={card_type}")

    JPN_CARDS.write_text(json.dumps(jpn, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"loc entries updated: {loc_changed}")
    print(f"cs files changed: {cs_changed}")
    if missing:
        print("MISSING:")
        for m in missing:
            print(" ", m)


if __name__ == "__main__":
    main()
