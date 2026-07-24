# -*- coding: utf-8 -*-
"""Card audit: CSV Nos 1-104 vs code + jpn localization. Read-only."""
import csv
import json
import re
import pathlib
from collections import defaultdict

ROOT = pathlib.Path(r"D:\Dev\antigravity\StSMod_N")
CSV_PATH = pathlib.Path(r"c:\Users\homut\Downloads\2026-07-25ヒプノクリエイター - シート1.csv")
OUT = ROOT / "_card_audit_report.txt"
CARDS_DIR = ROOT / "HypnosisCreatorCode" / "Cards"
LOC_PATH = ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json"

RARITY_MAP = {
    "スターター": "Basic",
    "ベーシック": "Basic",
    "基本": "Basic",
    "コモン": "Common",
    "アンコモン": "Uncommon",
    "レア": "Rare",
    "トークン": "Token",
    "スペシャル": "Special",
    "イベント": "Event",
}

TYPE_MAP = {
    "アタック": "Attack",
    "攻撃": "Attack",
    "スキル": "Skill",
    "パワー": "Power",
}


def parse_csv_cards():
    text = CSV_PATH.read_text(encoding="utf-8-sig")
    reader = csv.DictReader(text.splitlines())
    cols = reader.fieldnames
    rows = list(reader)

    name_col = "カード名称（日本語）"
    type_col = "種別"
    cost_col = "コスト"
    effect_col = "効果説明"
    ug_col = "UG時効果（変更点のみ記載）"
    rarity_col = "レア度"
    tag_col = "タグ"
    id_col = "id"
    note_col = "備考"
    build_col = "ビルド"

    card_rows = []
    for r in rows:
        no = (r.get("No") or "").strip()
        if not no.isdigit():
            continue
        n = int(no)
        if n < 1 or n > 104:
            continue
        name = (r.get(name_col) or "").strip()
        typ = (r.get(type_col) or "").strip()
        if typ == "心臓レリック":
            continue
        if not name:
            continue
        card_rows.append(
            {
                "No": n,
                "name": name,
                "type": typ,
                "build": (r.get(build_col) or "").strip(),
                "cost": (r.get(cost_col) or "").strip(),
                "effect": (r.get(effect_col) or "").strip(),
                "ug": (r.get(ug_col) or "").strip(),
                "rarity": (r.get(rarity_col) or "").strip(),
                "tags": (r.get(tag_col) or "").strip(),
                "id": (r.get(id_col) or "").strip(),
                "note": (r.get(note_col) or "").strip(),
            }
        )
    return card_rows, cols


def parse_card_classes():
    cards = []
    for path in sorted(CARDS_DIR.rglob("*.cs")):
        if path.name == "HypnosisCreatorCard.cs":
            continue
        text = path.read_text(encoding="utf-8")
        m = re.search(r"\b(?:abstract\s+)?class\s+(\w+)", text)
        if not m:
            continue
        cls = m.group(1)
        if re.search(r"\babstract\s+class\s+" + re.escape(cls), text):
            continue  # skip bases like TrainingCommand

        # Primary ctor: HypnosisCreatorCard(COST, CardType.X, CardRarity.Y, ...)
        ctor = re.search(
            r":\s*HypnosisCreatorCard\s*\(\s*"
            r"(-?\d+|X)\s*,\s*"
            r"CardType\.(\w+)\s*,\s*"
            r"CardRarity\.(\w+)",
            text,
            re.S,
        )
        # Token commands: : TrainingCommand or : TrainingCommand(TargetType.X, CardType.Y)
        tcmd = re.search(
            r":\s*TrainingCommand(?:\s*\(\s*"
            r"(?:TargetType\.\w+\s*)?"
            r"(?:,\s*)?"
            r"(?:CardType\.(\w+))?"
            r"[^)]*\))?",
            text,
        )
        if ctor:
            cost, ctype, rarity = ctor.group(1), ctor.group(2), ctor.group(3)
        elif path.parent.name == "Token" and "TrainingCommand" in text:
            cost, rarity = "0", "Token"
            # TrainingCommand(type: CardType.Attack) named arg, or positional
            tm = re.search(r"TrainingCommand\s*\([^;{]*CardType\.(\w+)", text, re.S)
            ctype = tm.group(1) if tm else "Skill"
        else:
            rarity_m = re.search(r"CardRarity\.(\w+)", text)
            type_m = re.search(r"CardType\.(\w+)", text)
            cost_m = re.search(r"HypnosisCreatorCard\s*\(\s*(-?\d+|X)", text)
            rarity = rarity_m.group(1) if rarity_m else "?"
            ctype = type_m.group(1) if type_m else "?"
            cost = cost_m.group(1) if cost_m else "?"

        # Loc key from Id if present, else derive from class via file IdEntry pattern
        # CustomCardModel typically uses class name; BaseLib generates HYPNOSISCREATOR-SNAKE
        # Extract from comments or Portrait path hints; prefer scanning for known loc keys later
        snake_m = re.search(
            r'CustomPortraitPath|PortraitPath|Id\.Entry|"([A-Z0-9_]+)\.png"',
            text,
        )
        # Better: convert CamelCase; then override from loc match
        snake = camel_to_snake(cls)
        # Known specials matching localization keys
        SPECIAL = {
            "HcDefend": "HC_DEFEND",
            "Sensitivity3000": "SENSITIVITY3000",
            "MetronomeCard": "METRONOME_CARD",
            "TrainingCommandCard": "TRAINING_COMMAND",  # may differ
            "TrainingCommand": "TRAINING_COMMAND",
        }
        if cls in SPECIAL:
            snake = SPECIAL[cls]

        folder = path.parent.name
        cards.append(
            {
                "class": cls,
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "folder": folder,
                "rarity": rarity,
                "type": ctype,
                "cost": str(cost),
                "snake": snake,
                "loc_key": f"HYPNOSISCREATOR-{snake}",
                "text": text,
            }
        )
    return cards


def camel_to_snake(name: str) -> str:
    s1 = re.sub(r"(.)([A-Z][a-z]+)", r"\1_\2", name)
    s2 = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", s1)
    # digit boundaries: Sensitivity3000 already special-cased
    return s2.upper()


def normalize_desc(s: str) -> str:
    if s is None:
        return ""
    return s.replace("\r\n", "\n").replace("\r", "\n").strip()


# Manual aliases CSV name -> class when titles differ (CSV vs loc / design rename)
CSV_NAME_TO_CLASS = {
    "Kneel!": "Kneel",
    "Look!": "Look",
    "Come!": "Come",
    "Relax!": "Relax",
    "Present!": "Present",
    "Trance!": "Trance",
    "Crawl!": "Crawl",
    "Don't Move!": "DontMove",
    "Roll!": "Roll",
    "Cum!": "Cum",
    "Good!": "Good",
    "ティンシャ": "Tingsha",
    "チンシャ": "Tingsha",
    "フリーハグ<3": "FreeHug",
    "フリーハグ♡": "FreeHug",
    "フリーハグ❤": "FreeHug",
    # Title variants (CSV longer / different wording vs loc)
    "認知シャッフル催眠": "CognitiveShuffle",
    "ポリネシアン催眠": "PolynesianHypnosis",
    "糸色 丁頁 イ崔 目民": "InfiniteUpgradeString",  # CSV garbled; loc=糸色丁頁
    "性癖の覇者": "FetishChampion",
    "オールインワン催眠": "AllInOne",
    "エリクソン的誘導": "Ericksonian",
    "ゼロへの近道": "ZeroShortcut",
    "過集中": "OverFocus",
    "底なしの沼": "BottomlessBog",
    "術式の開示": "RitualReveal",
}


def resolve_loc_keys(code_cards, loc):
    """Fix snake/loc_key by matching available loc titles and class-derived keys."""
    title_to_key = {}
    for k, v in loc.items():
        if k.endswith(".title") and k.startswith("HYPNOSISCREATOR-"):
            base = k[: -len(".title")]
            title_to_key.setdefault(v, []).append(base)

    # Also index all keys
    all_bases = {k[: -len(".title")] for k in loc if k.endswith(".title")}

    for c in code_cards:
        if c["loc_key"] in all_bases:
            continue
        # try common variants
        candidates = [
            c["loc_key"],
            f"HYPNOSISCREATOR-{camel_to_snake(c['class'])}",
        ]
        # TrainingCommandCard -> TRAINING_COMMAND_CARD or TRAINING_COMMAND
        if c["class"].endswith("Card") and c["class"] != "MetronomeCard":
            candidates.append(
                f"HYPNOSISCREATOR-{camel_to_snake(c['class'][:-4])}"
            )
        for cand in candidates:
            if cand in all_bases:
                c["loc_key"] = cand
                c["snake"] = cand.replace("HYPNOSISCREATOR-", "")
                break


def main():
    csv_cards, cols = parse_csv_cards()
    loc = json.loads(LOC_PATH.read_text(encoding="utf-8"))
    code_cards = parse_card_classes()
    resolve_loc_keys(code_cards, loc)

    for c in code_cards:
        c["loc_title"] = loc.get(c["loc_key"] + ".title")
        c["loc_desc"] = loc.get(c["loc_key"] + ".description")
        # If still missing title, search by scanning all titles won't help without class hint
        if c["loc_title"] is None:
            # try every key that contains class snake pieces
            snake = camel_to_snake(c["class"])
            for base in [f"HYPNOSISCREATOR-{snake}", c["loc_key"]]:
                if base + ".title" in loc:
                    c["loc_key"] = base
                    c["loc_title"] = loc[base + ".title"]
                    c["loc_desc"] = loc.get(base + ".description")
                    break

    # Build title index for matching
    by_title = defaultdict(list)
    for c in code_cards:
        if c["loc_title"]:
            by_title[c["loc_title"]].append(c)
    by_class = {c["class"]: c for c in code_cards}

    used = set()
    mappings = []

    for row in csv_cards:
        match = None
        # alias
        alias_cls = CSV_NAME_TO_CLASS.get(row["name"])
        if alias_cls and alias_cls in by_class and alias_cls not in used:
            match = by_class[alias_cls]
        # exact title
        if match is None:
            for c in by_title.get(row["name"], []):
                if c["class"] not in used:
                    match = c
                    break
        # normalized spaces
        if match is None:
            rn = row["name"].replace(" ", "").replace("　", "")
            for c in code_cards:
                if c["class"] in used or not c["loc_title"]:
                    continue
                if c["loc_title"].replace(" ", "").replace("　", "") == rn:
                    match = c
                    break
        # free hug heart variants
        if match is None and row["name"].startswith("フリーハグ"):
            for c in code_cards:
                if c["class"] == "FreeHug" and c["class"] not in used:
                    match = c
                    break
        # Tingsha: ティンシャ vs チンシャ
        if match is None and row["name"] in ("ティンシャ", "チンシャ"):
            for c in code_cards:
                if c["class"] == "Tingsha" and c["class"] not in used:
                    match = c
                    break

        issues = []
        if match is None:
            issues.append("MISSING_IN_CODE")
            mappings.append({"csv": row, "code": None, "issues": issues})
            continue

        used.add(match["class"])

        # Name
        if (match["loc_title"] or "") != row["name"]:
            issues.append(f"NAME: loc='{match['loc_title']}' csv='{row['name']}'")

        # Rarity
        expected = RARITY_MAP.get(row["rarity"])
        if row["rarity"] and expected is None:
            issues.append(f"RARITY_UNKNOWN_CSV: '{row['rarity']}'")
        elif expected:
            code_r = match["rarity"]
            if expected == "Basic":
                ok = code_r == "Basic"
            elif expected == "Token":
                ok = code_r == "Token" or match["folder"] == "Token"
            else:
                ok = code_r == expected
            # CSV lists command cards Nos.56-66 as コモン but code uses CardRarity.Token
            # Still report as mismatch (design vs implementation)
            if not ok:
                issues.append(
                    f"RARITY: csv={row['rarity']}→{expected} code=CardRarity.{code_r} folder={match['folder']}"
                )

        # Cost
        csv_cost = row["cost"]
        code_cost = match["cost"]
        if csv_cost != "" and str(code_cost) != str(csv_cost):
            if not (csv_cost.upper() == "X" and str(code_cost).upper() == "X"):
                issues.append(f"COST: csv={csv_cost} code={code_cost}")

        # Description exact
        loc_d = normalize_desc(match["loc_desc"] or "")
        csv_d = normalize_desc(row["effect"])
        if loc_d != csv_d:
            if csv_d and csv_d in loc_d:
                issues.append("DESC_MISMATCH (loc has extras beyond CSV)")
            elif loc_d and loc_d in csv_d:
                issues.append("DESC_MISMATCH (CSV has extras beyond loc)")
            else:
                issues.append("DESC_MISMATCH")

        # Type
        expected_t = TYPE_MAP.get(row["type"])
        if expected_t and match["type"] != expected_t:
            issues.append(
                f"TYPE: csv={row['type']}→{expected_t} code=CardType.{match['type']}"
            )

        mappings.append({"csv": row, "code": match, "issues": issues})

    extras = [c for c in code_cards if c["class"] not in used]

    rarity_n = sum(1 for m in mappings if any(i.startswith("RARITY:") for i in m["issues"]))
    name_n = sum(1 for m in mappings if any(i.startswith("NAME:") for i in m["issues"]))
    desc_n = sum(1 for m in mappings if any(i.startswith("DESC_MISMATCH") for i in m["issues"]))
    cost_n = sum(1 for m in mappings if any(i.startswith("COST:") for i in m["issues"]))
    type_n = sum(1 for m in mappings if any(i.startswith("TYPE:") for i in m["issues"]))
    missing_n = sum(1 for m in mappings if "MISSING_IN_CODE" in m["issues"])
    ok_n = sum(1 for m in mappings if not m["issues"])

    lines = []
    w = lines.append
    w("=" * 80)
    w("HYPNO CREATOR — CARD DISCREPANCY REPORT (CSV Nos 1–104 vs Code + jpn loc)")
    w("=" * 80)
    w(f"CSV: {CSV_PATH}")
    w(f"Encoding: UTF-8 with BOM")
    w(f"CSV columns: {cols}")
    w(f"CSV card rows (No.1–104): {len(csv_cards)}")
    w(f"Code card classes: {len(code_cards)} (excl. HypnosisCreatorCard.cs)")
    w(f"Loc .title entries: {sum(1 for k in loc if k.endswith('.title'))}")
    w("")
    w("COUNTS")
    w(f"  fully OK (no issues):      {ok_n}")
    w(f"  rarity mismatches:         {rarity_n}")
    w(f"  name mismatches:           {name_n}")
    w(f"  cost mismatches:           {cost_n}")
    w(f"  type mismatches:           {type_n}")
    w(f"  description mismatches:    {desc_n}")
    w(f"  missing in code:           {missing_n}")
    w(f"  extra in code (unmatched): {len(extras)}")
    w("")
    w("NOTE: Description comparison is EXACT string match.")
    w("Loc typically uses StS markup ({Damage:diff()}, [gold]...[/gold]) while CSV is design prose;")
    w("most DESC_MISMATCH are expected format differences unless texts are meant to be identical.")
    w("")

    w("-" * 80)
    w("MAPPING TABLE (CSV No → Class → issues)")
    w("-" * 80)
    for m in mappings:
        row = m["csv"]
        code = m["code"]
        if code:
            head = (
                f"No.{row['No']:3d} | CSV:{row['name']} | Class:{code['class']} ({code['folder']}) "
                f"| locTitle:{code['loc_title']} | CardRarity.{code['rarity']} cost={code['cost']} CardType.{code['type']}"
            )
        else:
            head = f"No.{row['No']:3d} | CSV:{row['name']} | Class:(NONE)"
        w(head)
        w(
            f"       CSV rarity={row['rarity']} cost={row['cost']} type={row['type']} build={row['build']}"
        )
        w(f"       ISSUES: {'; '.join(m['issues']) if m['issues'] else 'OK'}")

    w("")
    w("-" * 80)
    w("MISSING IN CODE (CSV rows with no matching class)")
    w("-" * 80)
    for m in mappings:
        if "MISSING_IN_CODE" in m["issues"]:
            row = m["csv"]
            w(
                f"  No.{row['No']} {row['name']} | {row['type']} cost={row['cost']} {row['rarity']} | effect={row['effect'][:80]}..."
            )

    w("")
    w("-" * 80)
    w("EXTRA CODE CARDS (not matched to CSV 1–104)")
    w("-" * 80)
    for c in extras:
        w(
            f"  {c['class']} | {c['path']} | CardRarity.{c['rarity']} cost={c['cost']} CardType.{c['type']} | locTitle={c['loc_title']!r}"
        )

    w("")
    w("-" * 80)
    w("RARITY MISMATCHES — recommended CardRarity")
    w("-" * 80)
    for m in mappings:
        if not any(i.startswith("RARITY:") for i in m["issues"]):
            continue
        row, code = m["csv"], m["code"]
        exp = RARITY_MAP.get(row["rarity"])
        w(
            f"  No.{row['No']:3d} {code['class']}: CardRarity.{code['rarity']} → CardRarity.{exp}  (CSV {row['rarity']}; folder={code['folder']})"
        )

    w("")
    w("-" * 80)
    w("COST MISMATCHES — recommended constructor cost")
    w("-" * 80)
    for m in mappings:
        if not any(i.startswith("COST:") for i in m["issues"]):
            continue
        row, code = m["csv"], m["code"]
        w(
            f"  No.{row['No']:3d} {code['class']}: HypnosisCreatorCard({code['cost']}, ...) → HypnosisCreatorCard({row['cost']}, ...)"
        )

    w("")
    w("-" * 80)
    w("NAME MISMATCHES")
    w("-" * 80)
    for m in mappings:
        for i in m["issues"]:
            if i.startswith("NAME:"):
                w(f"  No.{m['csv']['No']:3d} {m['code']['class']}: {i}")

    w("")
    w("-" * 80)
    w("TYPE MISMATCHES")
    w("-" * 80)
    for m in mappings:
        for i in m["issues"]:
            if i.startswith("TYPE:"):
                w(f"  No.{m['csv']['No']:3d} {m['code']['class']}: {i}")

    w("")
    w("-" * 80)
    w("DESCRIPTION MISMATCHES — FULL CSV 効果説明 + recommended loc string")
    w("-" * 80)
    for m in mappings:
        if not any(i.startswith("DESC_MISMATCH") for i in m["issues"]):
            continue
        row, code = m["csv"], m["code"]
        if not code:
            continue
        tag = [i for i in m["issues"] if i.startswith("DESC_MISMATCH")][0]
        w("")
        w(f"### No.{row['No']} {row['name']} → {code['class']}  [{tag}]")
        w(f"loc_key: {code['loc_key']}.description")
        w("--- CSV 効果説明 (exact) ---")
        w(row["effect"])
        w("--- CURRENT LOC description ---")
        w(code["loc_desc"] if code["loc_desc"] else "(missing)")
        w("--- RECOMMENDED (exact from CSV) ---")
        w(json.dumps({code["loc_key"] + ".description": row["effect"]}, ensure_ascii=False))
        w("--- END ---")

    w("")
    w("-" * 80)
    w("COMBINED CONSTRUCTOR FIX LIST (rarity+cost only)")
    w("-" * 80)
    for m in mappings:
        row, code = m["csv"], m["code"]
        if not code:
            continue
        parts = []
        exp = RARITY_MAP.get(row["rarity"])
        if exp and any(i.startswith("RARITY:") for i in m["issues"]):
            parts.append(f"CardRarity.{exp}")
        else:
            parts.append(f"CardRarity.{code['rarity']}(keep)")
        if any(i.startswith("COST:") for i in m["issues"]):
            parts.append(f"cost={row['cost']}")
        else:
            parts.append(f"cost={code['cost']}(keep)")
        if any(i.startswith("RARITY:") or i.startswith("COST:") for i in m["issues"]):
            w(f"  No.{row['No']:3d} {code['class']}: " + ", ".join(parts))

    report = "\n".join(lines) + "\n"
    OUT.write_text(report, encoding="utf-8")
    print(f"Wrote {OUT}")
    print(
        f"OK={ok_n} rarity={rarity_n} name={name_n} cost={cost_n} type={type_n} desc={desc_n} missing={missing_n} extras={len(extras)}"
    )
    # sanity: cost parse sample
    for name in ("TotalControl", "BreathControl", "ZeroOut", "AbnormalTransform"):
        c = by_class.get(name)
        if c:
            print(f"  {name}: cost={c['cost']} rarity={c['rarity']} type={c['type']} title={c['loc_title']}")


if __name__ == "__main__":
    main()
