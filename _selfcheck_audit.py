# -*- coding: utf-8 -*-
"""
Self-check: CSV Nos 1-104 vs current C# + jpn localization.
Produces structured findings (gameplay-focused). Read-only of game code;
writes only report artifacts under repo root.
"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
CSV = json.loads((ROOT / "_csv_cards_1_104.json").read_text(encoding="utf-8"))
JPN = json.loads(
    (ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json").read_text(
        encoding="utf-8"
    )
)
AUDIT = (ROOT / "_card_audit_report.txt").read_text(encoding="utf-8")
CARDS = ROOT / "HypnosisCreatorCode" / "Cards"

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
ALIASES = {
    "Sensitivity3000": "SENSITIVITY3000",
    "HcDefend": "HC_DEFEND",
    "SayYoureSorry": "SAY_YOURE_SORRY",
    "DontMove": "DONT_MOVE",
    "MetronomeCard": "METRONOME_CARD",
    "TrainingCommandCard": "TRAINING_COMMAND_CARD",
}

MAPPING = {
    int(m.group(1)): m.group(2)
    for m in re.finditer(r"No\.\s*(\d+)\s*\|\s*CSV:.*?\|\s*Class:(\w+)\s*\(", AUDIT)
}


def pascal_to_snake(name: str) -> str:
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).upper()


def entry_for(cls: str) -> str:
    return ALIASES.get(cls, pascal_to_snake(cls))


def find_file(cls: str) -> Path | None:
    hits = list(CARDS.rglob(f"{cls}.cs"))
    return hits[0] if hits else None


FETISH_MAP = {
    "SM": "Sm",
    "DomSub": "DomSub",
    "アブノーマル": "Abnormal",
    "トランス": "Trance",
}


def csv_fetishes(build: str) -> set[str]:
    out: set[str] = set()
    if not build:
        return out
    for m in re.finditer(r"性癖：\s*([^\s,，]+)", build):
        key = m.group(1).strip()
        if key in FETISH_MAP:
            out.add(FETISH_MAP[key])
    return out


def strip_markup(s: str) -> str:
    if not s:
        return ""
    s = re.sub(r"\{[^}]+\}", "", s)
    s = re.sub(r"\[[^\]]+\]", "", s)
    s = s.replace("\n", "").replace(" ", "").replace("　", "")
    return s


def normalize_jp(s: str) -> str:
    if not s:
        return ""
    s = s.replace("\r\n", "\n").replace("\r", "\n")
    s = re.sub(r"\s+", "", s)
    return s


def extract_card(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    d: dict = {"path": str(path.relative_to(ROOT)).replace("\\", "/"), "raw": text}

    m = re.search(
        r"HypnosisCreatorCard\s*\(\s*(-?\d+|CardCost\.X|X)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)",
        text,
    )
    if m:
        cost = m.group(1)
        if "X" in cost:
            cost = "X"
        d["cost"] = cost
        d["type"] = m.group(2)
        d["rarity"] = m.group(3)
    elif "TrainingCommand(" in text:
        d["cost"] = "0"
        d["type"] = "Skill"
        d["rarity"] = "Token"

    # folder rarity hint
    parts = path.parts
    for p in parts:
        if p in ("Basic", "Common", "Uncommon", "Rare", "Token"):
            d["folder"] = p

    d["keywords"] = sorted(set(re.findall(r"CardKeyword\.(\w+)", text)))
    d["fetishes"] = set(re.findall(r"FetishType\.(\w+)", text))
    # CardFetishes property only
    fm = re.search(
        r"CardFetishes\s*=>\s*(\[[^\]]*\]|new[^\n;]+)",
        text,
        re.S,
    )
    if fm:
        d["card_fetishes"] = set(re.findall(r"FetishType\.(\w+)", fm.group(1)))
    else:
        d["card_fetishes"] = set()

    vars_map: dict[str, float] = {}
    for m in re.finditer(r"new\s+DamageVar\s*\(\s*([\d.]+)M?", text):
        vars_map["damage"] = float(m.group(1))
    for m in re.finditer(r"new\s+BlockVar\s*\(\s*([\d.]+)M?", text):
        vars_map["block"] = float(m.group(1))
    for m in re.finditer(r'new\s+DynamicVar\s*\(\s*"(\w+)"\s*,\s*([\d.]+)M?', text):
        vars_map[m.group(1).lower()] = float(m.group(2))
    for m in re.finditer(r"new\s+PowerVar<(\w+)>\s*\(\s*([\d.]+)M?", text):
        pname = m.group(1).replace("Power", "").lower()
        vars_map[pname] = float(m.group(2))
    for m in re.finditer(
        r'new\s+PowerVar<(\w+)>\s*\(\s*"(\w+)"\s*,\s*([\d.]+)M?', text
    ):
        vars_map[m.group(2).lower()] = float(m.group(3))
    d["vars"] = vars_map

    # OnUpgrade body
    ou = re.search(
        r"protected\s+override\s+void\s+OnUpgrade\s*\(\s*\)\s*\{([\s\S]*?)\n\s*\}",
        text,
    )
    ou_body = ou.group(1) if ou else ""
    # also expression-bodied
    ou2 = re.search(
        r"protected\s+override\s+void\s+OnUpgrade\s*\(\s*\)\s*=>\s*([^;]+);", text
    )
    if ou2:
        ou_body += "\n" + ou2.group(1)

    d["on_upgrade"] = ou_body.strip()
    d["has_on_upgrade"] = bool(ou_body.strip()) and ou_body.strip() not in ("{}",)

    ug_deltas: dict[str, float] = {}
    for m in re.finditer(
        r'DynamicVars(?:\.(\w+)|\["(\w+)"\])\.UpgradeValueBy\s*\(\s*(-?[\d.]+)M?',
        ou_body,
    ):
        key = (m.group(1) or m.group(2) or "").lower()
        ug_deltas[key] = float(m.group(3))
    d["ug_deltas"] = ug_deltas

    em = re.search(r"EnergyCost\.UpgradeBy\s*\(\s*(-?\d+)\s*\)", ou_body)
    if em:
        d["ug_cost_delta"] = int(em.group(1))

    d["ug_add_retain"] = "AddKeyword(CardKeyword.Retain)" in ou_body
    d["ug_remove_exhaust"] = "RemoveKeyword(CardKeyword.Exhaust)" in ou_body
    d["ug_add_innate"] = "AddKeyword(CardKeyword.Innate)" in ou_body
    d["ug_add_exhaust"] = (
        "AddKeyword(CardKeyword.Exhaust)" in ou_body and not d["ug_remove_exhaust"]
    )

    # hardcoded power amounts in OnPlay
    hard: dict[str, list[int]] = {}
    for m in re.finditer(
        r"new\s+(\w+Power)\s*\(\s*(?:[^,)]*,\s*)?(\d+)", text
    ):
        hard.setdefault(m.group(1), []).append(int(m.group(2)))
    for m in re.finditer(r"ApplyPower.*?(\w+Power).*?(\d+)", text):
        hard.setdefault(m.group(1), []).append(int(m.group(2)))
    d["hard_powers"] = hard

    d["mentions"] = {
        "Confused": "ConfusedPower" in text or "Confused" in text,
        "StolenHeart": "StolenHeart" in text,
        "StatusHypnosis": "StatusHypnosis" in text,
        "TODO": "TODO" in text or "FIXME" in text or "近似" in text or "stub" in text.lower(),
        "Count": "CardKeyword.Count" in text or "IsCountCard" in text or "カウント" in text,
        "Retain": "CardKeyword.Retain" in text,
        "Exhaust": "CardKeyword.Exhaust" in text,
        "Innate": "CardKeyword.Innate" in text,
    }

    # OnPlay snippet for summary
    op = re.search(
        r"protected\s+override\s+void\s+OnPlay\s*\([^{]*\{([\s\S]*?)\n\s*\}", text
    )
    d["on_play"] = (op.group(1).strip()[:800] if op else "")

    return d


# --- CSV number extraction ---
NUM_PATTERNS = [
    (r"(\d+(?:\.\d+)?)\s*ダメージ", "damage"),
    (r"(\d+(?:\.\d+)?)\s*ブロック", "block"),
    (r"破滅\s*(\d+(?:\.\d+)?)", "doom"),
    (r"トランス\s*(\d+(?:\.\d+)?)", "trance"),
    (r"毒\s*(\d+(?:\.\d+)?)", "poison"),
    (r"弱体\s*(\d+)", "vulnerable"),
    (r"脱力\s*(\d+)", "weak"),
    (r"沼\s*(\d+)", "bog"),
    (r"睡眠\s*(\d+)", "sleep"),
    (r"締め付け\s*(\d+)", "constrict"),
    (r"活力\s*(\d+)", "vigor"),
    (r"敏捷\s*(\d+)", "dexterity"),
    (r"筋力\s*(\d+)", "strength"),
    (r"(\d+)\s*枚", "cards"),
    (r"(\d+)\s*ゴールド", "gold"),
]


def parse_numbers(text: str) -> dict[str, list[float]]:
    out: dict[str, list[float]] = {}
    if not text:
        return out
    for pat, key in NUM_PATTERNS:
        for m in re.finditer(pat, text):
            out.setdefault(key, []).append(float(m.group(1)))
    return out


def csv_has_keyword(effect: str, ug: str, build: str, notes: str) -> dict[str, bool]:
    blob = " ".join([effect or "", ug or "", build or "", notes or ""])
    return {
        "count": "カウント" in blob or "カウント" in (build or ""),
        "retain": "保留" in blob,
        "exhaust": "廃棄" in blob,
        "innate": "天賦" in blob,
    }


# Known intentional stubs from task.md
STUBS = {20: "LoveHypnosis", 18: "StatusHypnosis", 96: "HeartbeatShare"}

confirmed: list[dict] = []
approx: list[dict] = []
needs: list[dict] = []
ok_cards: list[int] = []

# Cards to deep-check from prior ug report + cost/type/rarity
PRIORITY_NOS = set(range(1, 105))

for row in CSV:
    no = row["no"]
    cls = MAPPING.get(no)
    if not cls:
        confirmed.append(
            {
                "no": no,
                "name": row["name"],
                "cls": "?",
                "cat": "mapping",
                "problem": "マッピング欠落",
                "csv": "-",
                "impl": "-",
            }
        )
        continue

    path = find_file(cls)
    if not path:
        confirmed.append(
            {
                "no": no,
                "name": row["name"],
                "cls": cls,
                "cat": "missing",
                "problem": "C# ファイルなし",
                "csv": "-",
                "impl": "-",
            }
        )
        continue

    code = extract_card(path)
    entry = entry_for(cls)
    title = JPN.get(f"HYPNOSISCREATOR-{entry}.title")
    desc = JPN.get(f"HYPNOSISCREATOR-{entry}.description", "")

    issues: list[dict] = []
    soft: list[dict] = []

    # A. Cost / Type / Rarity
    expect_rar = "Token" if cls in TOKENS else RARITY.get(row["rarity"])
    expect_type = TYPE.get(row["type"])
    expect_cost = "X" if str(row["cost"]).upper() == "X" else str(row["cost"]).strip()
    # Token cards: skip cost/type/rarity rigid check if Token
    if cls not in TOKENS:
        if code.get("cost") != expect_cost:
            issues.append(
                {
                    "cat": "A-cost",
                    "problem": "コスト不一致",
                    "csv": expect_cost,
                    "impl": str(code.get("cost")),
                }
            )
        if code.get("type") != expect_type:
            issues.append(
                {
                    "cat": "A-type",
                    "problem": "種別不一致",
                    "csv": f"{row['type']}→{expect_type}",
                    "impl": str(code.get("type")),
                }
            )
        if code.get("rarity") != expect_rar:
            issues.append(
                {
                    "cat": "A-rarity",
                    "problem": "レア度不一致",
                    "csv": f"{row['rarity']}→{expect_rar}",
                    "impl": str(code.get("rarity")),
                }
            )

    # B. JP title
    if title != row["name"]:
        issues.append(
            {
                "cat": "B-title",
                "problem": "日本語タイトル不一致",
                "csv": row["name"],
                "impl": title or "(missing)",
            }
        )

    # C. JP description vs CSV effect (exact after normalize; flag drift)
    # Loc often has markup — compare stripped
    csv_eff = normalize_jp(row["effect"])
    loc_plain = normalize_jp(strip_markup(desc))
    # Also compare without markup removal of CSV
    if csv_eff and loc_plain and csv_eff != loc_plain:
        # Check if loc contains core of csv (allow markup extras)
        # Flag as description drift only if clearly different numbers or missing key phrases
        soft.append(
            {
                "cat": "C-desc",
                "problem": "JP説明がCSV効果と完全一致しない（マークアップ差の可能性）",
                "csv": row["effect"][:80],
                "impl": (desc or "")[:80],
            }
        )

    # E. Fetish tags
    want_f = csv_fetishes(row.get("build", ""))
    have_f = code.get("card_fetishes") or set()
    # Awaken calls don't count as CardFetishes
    if want_f != have_f:
        # Trance in build as ビルド「トランス」without 性癖： may mean keyword not fetish
        build = row.get("build", "")
        # If CSV only has 性癖： tags, compare those
        if want_f or have_f:
            if want_f - have_f or have_f - want_f:
                issues.append(
                    {
                        "cat": "E-fetish",
                        "problem": "性癖タグ不一致",
                        "csv": ",".join(sorted(want_f)) or "(なし)",
                        "impl": ",".join(sorted(have_f)) or "(なし)",
                    }
                )

    # F. Keywords from CSV effect/ug/build
    kw = csv_has_keyword(
        row.get("effect", ""),
        row.get("ug", ""),
        row.get("build", ""),
        row.get("notes", ""),
    )
    code_kw = set(code.get("keywords") or [])
    # Count: CSV build or effect starts with カウント
    if "カウント" in (row.get("build") or "") or (
        row.get("effect") or ""
    ).startswith("カウント"):
        if "Count" not in code_kw and "IsCount" not in code["raw"]:
            # check for Count keyword via base helper
            if "CardKeyword.Count" not in code["raw"] and "CountCard" not in code["raw"]:
                soft.append(
                    {
                        "cat": "F-count",
                        "problem": "CSVはカウントだがコードにCountキーワードが見当たらない",
                        "csv": "カウント",
                        "impl": str(code_kw),
                    }
                )

    if "廃棄" in (row.get("effect") or "") and "廃棄が消える" not in (
        row.get("ug") or ""
    ):
        # base should have Exhaust
        if "Exhaust" not in code_kw and "廃棄" in (row.get("effect") or ""):
            # many cards say 廃棄 at end
            if re.search(r"廃棄[。．]?$", row.get("effect") or ""):
                if "Exhaust" not in code_kw:
                    issues.append(
                        {
                            "cat": "F-exhaust",
                            "problem": "CSVは廃棄付きだがコードにExhaustなし",
                            "csv": "廃棄",
                            "impl": str(code_kw),
                        }
                    )

    # D. Numeric: compare primary damage/block/trance/doom if both present
    csv_nums = parse_numbers(row.get("effect", ""))
    ug_nums = parse_numbers(row.get("ug", ""))
    vars_ = code.get("vars") or {}

    def first(d, k):
        xs = d.get(k) or []
        return xs[0] if xs else None

    for key, vkey in [
        ("damage", "damage"),
        ("block", "block"),
        ("doom", "doom"),
        ("trance", "trance"),
        ("poison", "poison"),
        ("constrict", "constrict"),
        ("vigor", "vigor"),
        ("bog", "bog"),
        ("sleep", "sleep"),
        ("vulnerable", "vulnerable"),
        ("weak", "weak"),
        ("strength", "strength"),
    ]:
        cv = first(csv_nums, key)
        iv = vars_.get(vkey)
        # also try alternate keys
        if iv is None:
            for alt in vars_:
                if key in alt or alt in key:
                    iv = vars_[alt]
                    break
        if cv is not None and iv is not None and abs(cv - iv) > 0.01:
            # trance often appears as "トランス3以上" requirement — skip if 以上
            if key == "trance" and "トランス" in (row.get("effect") or "") and "以上" in (
                row.get("effect") or ""
            ):
                continue
            issues.append(
                {
                    "cat": f"D-{key}",
                    "problem": f"数値不一致({key})",
                    "csv": str(cv),
                    "impl": f"{iv} (CanonicalVars)",
                }
            )

    # UG cost change
    ug_text = row.get("ug") or ""
    if re.search(r"コストが?1に|1コスト", ug_text) and "0コスト" not in ug_text:
        if code.get("ug_cost_delta") != -1 and expect_cost not in ("1",):
            # expected cost becomes 1
            try:
                base_c = int(expect_cost) if expect_cost.isdigit() else None
            except Exception:
                base_c = None
            if base_c is not None and base_c != 1:
                want_delta = 1 - base_c
                if code.get("ug_cost_delta") != want_delta:
                    soft.append(
                        {
                            "cat": "D-ug-cost",
                            "problem": "UGコスト変更の疑い",
                            "csv": ug_text,
                            "impl": f"ug_cost_delta={code.get('ug_cost_delta')}",
                        }
                    )
    if "0コスト" in ug_text or "コストが0" in ug_text:
        try:
            base_c = int(expect_cost) if expect_cost.isdigit() else None
        except Exception:
            base_c = None
        if base_c is not None and base_c != 0:
            want_delta = -base_c
            if code.get("ug_cost_delta") != want_delta:
                soft.append(
                    {
                        "cat": "D-ug-cost0",
                        "problem": "UGで0コストになるはず",
                        "csv": ug_text,
                        "impl": f"ug_cost_delta={code.get('ug_cost_delta')}",
                    }
                )
    if re.search(r"廃棄が?(?:消える|なくなる|消滅)", ug_text):
        if not code.get("ug_remove_exhaust"):
            issues.append(
                {
                    "cat": "F-ug-exhaust",
                    "problem": "UGで廃棄消滅のはずがRemoveKeyword(Exhaust)なし",
                    "csv": ug_text,
                    "impl": code.get("on_upgrade", "")[:100] or "(OnUpgrade空)",
                }
            )
    if re.search(r"保留", ug_text) and "消" not in ug_text:
        if not code.get("ug_add_retain") and "Retain" not in code_kw:
            soft.append(
                {
                    "cat": "F-ug-retain",
                    "problem": "UGで保留付与の疑い",
                    "csv": ug_text,
                    "impl": code.get("on_upgrade", "")[:100] or "(なし)",
                }
            )
    if "天賦" in ug_text:
        if not code.get("ug_add_innate") and "Innate" not in code_kw:
            issues.append(
                {
                    "cat": "F-ug-innate",
                    "problem": "UGで天賦のはずがInnateなし",
                    "csv": ug_text,
                    "impl": code.get("on_upgrade", "")[:100] or "(なし)",
                }
            )

    # G. Stubs
    if no in STUBS:
        approx.append(
            {
                "no": no,
                "name": row["name"],
                "cls": cls,
                "problem": "task.md 記載の意図的近似",
                "csv": row["effect"][:100],
                "impl": f"mentions={code['mentions']}; path={code['path']}",
            }
        )

    # Collect
    hard_issues = [i for i in issues if i["cat"] != "C-desc"]
    # Description exact match check separately
    if csv_eff == loc_plain or (
        csv_eff and loc_plain and csv_eff in loc_plain
    ) or (loc_plain and csv_eff and loc_plain in csv_eff):
        soft = [s for s in soft if s["cat"] != "C-desc"]
    elif desc and normalize_jp(desc) == csv_eff:
        soft = [s for s in soft if s["cat"] != "C-desc"]

    if hard_issues:
        for i in hard_issues:
            confirmed.append(
                {
                    "no": no,
                    "name": row["name"],
                    "cls": cls,
                    **i,
                }
            )
    for s in soft:
        needs.append({"no": no, "name": row["name"], "cls": cls, **s})

    if not hard_issues and no not in STUBS:
        # still may have soft
        if not soft:
            ok_cards.append(no)

# Dump intermediate for manual review
out = {
    "ok_count": len(ok_cards),
    "ok_cards": ok_cards,
    "confirmed": confirmed,
    "approx": approx,
    "needs": needs,
    "mapping_count": len(MAPPING),
}
(ROOT / "_selfcheck_audit_raw.json").write_text(
    json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8"
)

# Summary prints
from collections import Counter

print("OK (no hard/soft):", len(ok_cards))
print("Confirmed issues:", len(confirmed))
print("Approx stubs:", len(approx))
print("Needs review:", len(needs))
print("By cat:", Counter(i["cat"] for i in confirmed))
print("Needs by cat:", Counter(i["cat"] for i in needs))
print("\n--- CONFIRMED (first 60) ---")
for i in confirmed[:60]:
    print(
        f"No.{i['no']:3d} {i['name']} [{i['cat']}] {i['problem']}: csv={i['csv']!r} impl={i['impl']!r}"
    )
print(f"... total confirmed {len(confirmed)}")
print("\n--- NEEDS (non-desc, first 40) ---")
nd = [n for n in needs if n["cat"] != "C-desc"]
for i in nd[:40]:
    print(
        f"No.{i['no']:3d} {i['name']} [{i['cat']}] {i['problem']}: csv={i['csv']!r} impl={i['impl']!r}"
    )
print(f"needs non-desc: {len(nd)}; desc-only: {sum(1 for n in needs if n['cat']=='C-desc')}")
