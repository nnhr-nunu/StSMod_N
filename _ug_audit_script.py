#!/usr/bin/env python3
"""Audit Hypno Creator cards 1-104: CSV base/UG vs code CanonicalVars + OnUpgrade."""
import json
import re
from pathlib import Path
from dataclasses import dataclass, field

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
CSV_PATH = ROOT / "_csv_cards_1_104.json"
MAP_PATH = ROOT / "_card_audit_report.txt"
CARDS_DIR = ROOT / "HypnosisCreatorCode" / "Cards"
OUT_MD = ROOT / "_ug_audit_report.md"
OUT_JSON = ROOT / "_ug_audit_fixes.json"

# --- mapping ---
CLASS_MAP: dict[int, str] = {}
for line in MAP_PATH.read_text(encoding="utf-8").splitlines():
    m = re.match(r"No\.\s*(\d+)\s*\|.*Class:(\w+)", line)
    if m:
        CLASS_MAP[int(m.group(1))] = m.group(2)

csv_cards = json.loads(CSV_PATH.read_text(encoding="utf-8"))
csv_by_no = {c["no"]: c for c in csv_cards}

# find class file
class_files: dict[str, Path] = {}
for p in CARDS_DIR.rglob("*.cs"):
    if p.name == "HypnosisCreatorCard.cs":
        continue
    name = p.stem
    if name not in class_files:
        class_files[name] = p

# --- number extraction from Japanese text ---
NUM_PATTERNS = [
    (r"(\d+(?:\.\d+)?)\s*ダメージ", "damage"),
    (r"(\d+(?:\.\d+)?)\s*ブロック", "block"),
    (r"破滅\s*(\d+(?:\.\d+)?)", "doom"),
    (r"トランス\s*(\d+(?:\.\d+)?)", "trance"),
    (r"毒\s*(\d+(?:\.\d+)?)", "poison"),
    (r"筋力(?:低下|減)?\s*(\d+)", "strength_loss"),
    (r"弱体\s*(\d+)", "vulnerable"),
    (r"脱力\s*(\d+)", "weak"),
    (r"沼\s*(\d+)", "bog"),
    (r"睡眠\s*(\d+)", "sleep"),
    (r"締め付け\s*(\d+)", "constrict"),
    (r"HP\s*(\d+)\s*を?(?:失|回復)", "hp"),
    (r"体力\s*(\d+)\s*を?失", "hp_loss"),
    (r"(\d+)\s*回(?:与える|プレイ|引く|枚|回分)", "times"),
    (r"リプレイ\s*(\d+)", "replay"),
    (r"(\d+)\s*枚", "cards"),
    (r"(\d+)\s*ゴールド", "gold"),
    (r"アーティファクト\s*(\d+)", "artifact"),
    (r"縮小\s*(\d+)", "shrink"),
    (r"活力\s*(\d+)", "vigor"),
    (r"敏捷\s*(\d+)", "dexterity"),
    (r"筋力\s*(\d+)", "strength"),
    (r"(\d+)\s*コスト", "cost_mention"),
]

UG_KEYWORD_PATTERNS = [
    (r"廃棄が?(?:消える|なくなる|消滅)", "remove_exhaust"),
    (r"保留(?:が)?(?:つく|\.|$)", "add_retain"),
    (r"天賦", "innate"),
    (r"UGなし", "no_ug"),
    (r"(\d+)\s*コスト(?:に)?(?:なる|が)", "cost_to"),
    (r"0コスト", "cost_0"),
    (r"1コスト", "cost_1"),
    (r"2コスト", "cost_2"),
    (r"プレイ後(?:は)?山札", "shuffle_into_deck"),
    (r"手札に戻", "return_hand"),
    (r"全体", "aoe"),
    (r"相手すべて", "all_enemies"),
]


def parse_numbers(text: str) -> dict[str, list[float]]:
    out: dict[str, list[float]] = {}
    if not text:
        return out
    t = text.replace("、", ",").replace("。", ".")
    for pat, key in NUM_PATTERNS:
        for m in re.finditer(pat, t):
            val = float(m.group(1).replace(",", ""))
            out.setdefault(key, []).append(val)
    return out


def parse_ug_keywords(text: str) -> set[str]:
    s: set[str] = set()
    if not text:
        return s
    for pat, key in UG_KEYWORD_PATTERNS:
        if re.search(pat, text):
            s.add(key)
    return s


def extract_code(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    d: dict = {"file": str(path.relative_to(ROOT)).replace("\\", "/")}

    # cost from constructor
    cm = re.search(r":\s*HypnosisCreatorCard\s*\(\s*(\d+|CardCost\.X)", text)
    if cm:
        d["cost"] = "X" if "X" in cm.group(1) else int(cm.group(1))

    # keywords
    kw = re.findall(r"CardKeyword\.(\w+)", text)
    d["keywords"] = sorted(set(kw))

    # CanonicalVars numbers
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
    for m in re.finditer(r'new\s+PowerVar<(\w+)>\s*\(\s*"(\w+)"\s*,\s*([\d.]+)M?', text):
        vars_map[m.group(2).lower()] = float(m.group(3))

    d["vars"] = vars_map

    # OnUpgrade deltas
    ug = {}
    ou_match = re.search(r"protected\s+override\s+void\s+OnUpgrade\s*\(\s*\)\s*(?:=>|\{)([\s\S]*?)(?:\n\s*\}|\n\s*$)", text)
    ou_body = ou_match.group(1) if ou_match else ""
    if re.search(r"OnUpgrade\s*\(\s*\)\s*\{\s*\}", text) or "OnUpgrade() { }" in text.replace(" ", ""):
        ug["empty"] = True

    for m in re.finditer(r"DynamicVars\.(\w+|\[\"(\w+)\"\])\.UpgradeValueBy\s*\(\s*([\d.]+)M?", ou_body + text):
        key = (m.group(2) or m.group(1)).lower()
        ug.setdefault("upgrade_delta", {})[key] = float(m.group(3))
    for m in re.finditer(r'DynamicVars\["(\w+)"\]\.UpgradeValueBy\s*\(\s*([\d.]+)M?', ou_body + text):
        ug.setdefault("upgrade_delta", {})[m.group(1).lower()] = float(m.group(2))

    if "EnergyCost.UpgradeBy" in ou_body or "EnergyCost.UpgradeBy" in text:
        em = re.search(r"EnergyCost\.UpgradeBy\s*\(\s*(-?\d+)\s*\)", ou_body + text)
        if em:
            ug["cost_delta"] = int(em.group(1))

    if "AddKeyword(CardKeyword.Retain)" in text:
        ug["add_retain"] = True
    if "RemoveKeyword(CardKeyword.Exhaust)" in text:
        ug["remove_exhaust"] = True
    if "AddKeyword(CardKeyword.Exhaust)" in text and "RemoveKeyword" not in text:
        pass  # base keyword
    if "AddKeyword(CardKeyword.Innate)" in text or "AddKeyword(HcKeywords.Innate" in text:
        ug["add_innate"] = True

    d["upgrade"] = ug

    # upgraded values (base + delta)
    upgraded = dict(vars_map)
    for k, delta in ug.get("upgrade_delta", {}).items():
        if k in upgraded:
            upgraded[k] += delta
        elif k == "damage" and "damage" in upgraded:
            upgraded["damage"] += delta
    d["vars_upgraded"] = upgraded

    if "cost_delta" in ug and "cost" in d and d["cost"] != "X":
        d["cost_upgraded"] = d["cost"] + ug["cost_delta"]

    # special: replay in OnPlay
    if "Replay" in text or "replay" in text.lower():
        rm = re.search(r"Replay\s*\(\s*(\d+)", text, re.I)
        if rm:
            d["replay"] = int(rm.group(1))

    return d


@dataclass
class Issue:
    kind: str  # mismatch | needs_inference
    no: int
    name: str
    cls: str
    csv_base: str
    csv_ug: str
    code_current: str
    suggested: str
    severity: int = 5
    fixes: list = field(default_factory=list)


def infer_csv_ug_target(base: dict, ug_text: str, base_nums: dict) -> dict:
    """Infer UG target numbers when CSV ug only gives delta-ish text."""
    ug_nums = parse_numbers(ug_text)
    target = dict(base_nums)
    # If ug gives absolute numbers, use them
    for k, vals in ug_nums.items():
        if vals:
            target[k] = vals[-1]  # last mention often the changed value
    return target


def compare_card(no: int, csv: dict, code: dict, cls: str) -> tuple[list[Issue], bool]:
    """Returns (issues, is_ok)."""
    issues: list[Issue] = []
    effect = csv.get("effect", "")
    ug = csv.get("ug", "")
    base_nums = parse_numbers(effect)
    ug_nums = parse_numbers(ug)
    ug_kw = parse_ug_keywords(ug)
    code_vars = code.get("vars", {})
    code_up = code.get("vars_upgraded", {})

    name = csv.get("name", "")

    def add(kind, code_cur, suggested, severity=5, fixes=None):
        issues.append(Issue(
            kind=kind, no=no, name=name, cls=cls,
            csv_base=effect[:120], csv_ug=ug,
            code_current=code_cur, suggested=suggested,
            severity=severity, fixes=fixes or []
        ))

    # Map code var names to CSV keys
    VAR_ALIAS = {
        "damage": "damage",
        "block": "block",
        "doom": "doom",
        "trance": "trance",
        "poison": "poison",
        "strengthloss": "strength_loss",
        "vulnerable": "vulnerable",
        "weak": "weak",
        "bog": "bog",
        "cards": "cards",
    }

    # --- BASE comparisons ---
    for csv_key, code_keys in [
        ("damage", ["damage"]),
        ("block", ["block"]),
        ("doom", ["doom"]),
        ("trance", ["trance"]),
        ("poison", ["poison"]),
        ("vulnerable", ["vulnerable"]),
        ("weak", ["weak"]),
        ("bog", ["bog"]),
    ]:
        if csv_key not in base_nums:
            continue
        csv_val = base_nums[csv_key][0]
        code_val = None
        for ck in code_keys:
            if ck in code_vars:
                code_val = code_vars[ck]
                break
        if code_val is None:
            # check alternate names in code
            for k, v in code_vars.items():
                if csv_key in k or k in csv_key:
                    code_val = v
                    break
        if code_val is not None and abs(code_val - csv_val) > 0.01:
            add("mismatch",
                f"BASE {csv_key}={code_val}",
                f"BASE {csv_key}={int(csv_val) if csv_val == int(csv_val) else csv_val}",
                severity=8 if csv_key in ("damage", "block") else 6,
                fixes=[{"file": code["file"], "what": f"BASE {csv_key}",
                        "from": str(code_val), "to": str(int(csv_val) if csv_val == int(csv_val) else csv_val),
                        "ug_infer": False}]
            )

    # BASE exhaust keyword
    if "廃棄" in effect and "Exhaust" not in code.get("keywords", []):
        if "廃棄が" not in ug:  # ug removes exhaust
            add("mismatch", "BASE: Exhaust欠如", "Add CardKeyword.Exhaust", severity=4)

    # --- UG comparisons ---
    if ug_kw & {"no_ug"}:
        return issues, len([i for i in issues if i.kind == "mismatch"]) == 0

    if not ug or ug.strip() in ("", "—", "-"):
        return issues, len([i for i in issues if i.kind == "mismatch"]) == 0

    # UG keyword: remove exhaust
    if "remove_exhaust" in ug_kw:
        if not code.get("upgrade", {}).get("remove_exhaust"):
            add("mismatch", "UG: Exhaust削除なし", "OnUpgrade: RemoveKeyword(CardKeyword.Exhaust)", severity=7,
                fixes=[{"file": code["file"], "what": "UG remove Exhaust", "from": "none", "to": "RemoveKeyword(Exhaust)", "ug_infer": False}])

    # UG keyword: retain
    if "add_retain" in ug_kw:
        if not code.get("upgrade", {}).get("add_retain"):
            add("mismatch", "UG: Retainなし", "OnUpgrade: AddKeyword(CardKeyword.Retain)", severity=6,
                fixes=[{"file": code["file"], "what": "UG add Retain", "from": "none", "to": "AddKeyword(Retain)", "ug_infer": False}])

    # UG keyword: innate
    if "innate" in ug_kw:
        if not code.get("upgrade", {}).get("add_innate"):
            add("mismatch", "UG: Innate/天賦なし", "OnUpgrade: AddKeyword Innate", severity=5)

    # UG cost change
    cost_ug = re.search(r"(\d+)\s*コスト(?:に)?(?:なる|が)", ug)
    if cost_ug:
        target_cost = int(cost_ug.group(1))
        cur_up = code.get("cost_upgraded", code.get("cost"))
        cur_base = code.get("cost")
        if cur_up != target_cost and cur_base != target_cost:
            delta = target_cost - (cur_base if isinstance(cur_base, int) else 0)
            add("mismatch",
                f"UG cost={cur_up or cur_base}",
                f"UG cost={target_cost} (EnergyCost.UpgradeBy({delta}))",
                severity=7,
                fixes=[{"file": code["file"], "what": "UG cost", "from": str(cur_base), "to": str(target_cost), "ug_infer": False}])

    if "cost_0" in ug_kw and code.get("cost") != 0:
        up = code.get("cost_upgraded")
        if up != 0:
            add("mismatch", f"UG cost={up or code.get('cost')}", "UG cost=0", severity=7)

    # UG numeric - compare upgraded values
    # Determine expected UG values
    expected_ug: dict[str, float] = {}
    ug_infer = False

    # Full restatement in ug (e.g. DrugHypnosis)
    if len(ug_nums) >= 3:
        for k, vals in ug_nums.items():
            if k in ("damage", "block", "doom", "trance", "poison", "vulnerable", "weak", "bog", "sleep", "constrict", "hp", "gold", "artifact", "shrink"):
                expected_ug[k] = vals[0]
    else:
        # Delta inference: "8ダメージ" when base is 5 -> ug target 8
        for k, vals in ug_nums.items():
            if k in base_nums and len(vals) == 1 and len(base_nums[k]) == 1:
                # If ug number differs from base, it's likely absolute target
                if abs(vals[0] - base_nums[k][0]) > 0.01:
                    expected_ug[k] = vals[0]
                else:
                    expected_ug[k] = vals[0]
            elif k in ("damage", "block", "doom") and vals:
                expected_ug[k] = vals[0]
            elif k not in base_nums and vals:
                expected_ug[k] = vals[0]

        # Partial ug text - infer from delta phrases
        for k in ("damage", "block", "doom", "vulnerable", "weak", "bog", "poison"):
            if k in ug_nums and k not in expected_ug:
                expected_ug[k] = ug_nums[k][0]

        if not expected_ug and ug_nums:
            ug_infer = True

    CODE_UG_MAP = {
        "damage": "damage", "block": "block", "doom": "doom", "trance": "trance",
        "poison": "poison", "vulnerable": "vulnerable", "weak": "weak", "bog": "bog",
    }

    for csv_key, expected in expected_ug.items():
        ck = CODE_UG_MAP.get(csv_key, csv_key)
        actual = code_up.get(ck)
        if actual is None and ck in code_vars:
            # no upgrade delta but ug expects change
            actual = code_vars[ck]
        if actual is None:
            continue
        if abs(actual - expected) > 0.01:
            delta = expected - code_vars.get(ck, actual)
            kind = "needs_inference" if ug_infer or "……" in ug or "…" in ug else "mismatch"
            add(kind,
                f"UG {csv_key}={actual} (base={code_vars.get(ck)})",
                f"UG {csv_key}={int(expected) if expected == int(expected) else expected}",
                severity=9 if csv_key == "damage" else 7,
                fixes=[{"file": code["file"], "what": f"UG {csv_key}",
                        "from": str(actual), "to": str(int(expected) if expected == int(expected) else expected),
                        "ug_infer": ug_infer}]
            )

    # Special: only partial ug without numbers (behavior change)
    behavioral_ug = [
        ("全体攻撃", "aoe", "AOE化未実装の可能性"),
        ("相手すべて", "all_enemies", "全体対象化未実装の可能性"),
        ("山札に入る", "shuffle_into_deck", "UG: 山札シャッフル"),
        ("2枚選ぶ", "pick_2", "UG: 選択枚数2"),
        ("3枚生成", "spawn_3", "UG: 生成枚数3"),
        ("ランダムなアップグレード済み", "ug_cards", "UG: アップグレード済み変身"),
        ("ブロックを0に", "zero_block", "UG: 攻撃値とブロック0"),
        ("攻撃値とブロック", "zero_both", "UG: 攻撃値+ブロック0"),
        ("2枚引く", "draw_2", "UG: ドロー2"),
        ("カードを2枚引く", "draw_2", "UG: ドロー2"),
        ("デバフすべて", "cleanse_all", "UG: 全デバフ解除"),
        ("2枚の性癖", "plant_2", "UG: 性癖2枚"),
        ("5枚", "cards_5", "UG: 5枚生成"),
        ("リプレイ2", "replay_2", "UG: Replay 2"),
        ("HPを15失う", "hp_drain_15", "UG: HP15/turn"),
        ("HPを30回復", "heal_30", "UG: 回復30"),
        ("睡眠2", "sleep_2", "UG: 睡眠2"),
        ("相手全員", "all_enemies", "UG: 全員対象"),
        ("108", "artifact_108", "UG: Artifact 108"),
        ("2枚に", "enchant_2", "UG: 2枚エンチャント"),
        ("3枚になる", "cards_3", "UG: 3枚配布"),
        ("廃棄されたものも", "include_exhaust", "UG: 廃棄 pile含む"),
        ("エナジーを獲得し、同じ数だけカード", "energy_draw", "UG: エナジー+同数ドロー"),
        ("2枚のDomSub", "domsub_2", "UG: DomSub2枚"),
        ("15ダメージ", "doom_15_partial", "partial"),
    ]

    if not expected_ug and not (ug_kw & {"remove_exhaust", "add_retain", "innate", "no_ug"}):
        for phrase, tag, desc in behavioral_ug:
            if phrase in ug:
                # flag if OnUpgrade empty or doesn't address
                ou = code.get("upgrade", {})
                if ou.get("empty") or (len(ou) <= 1 and ou.get("empty")):
                    add("needs_inference" if "……" in ug or "…" in ug or "さらに" in ug else "mismatch",
                        f"OnUpgrade空/数値UGのみ: {list(ou.keys())}",
                        desc,
                        severity=6)
                    break
                elif tag.startswith("replay") and code.get("replay") != 2:
                    if "リプレイ2" in ug or "リプレイ 2" in ug:
                        add("mismatch", f"replay={code.get('replay')}", "UG: Replay 2", severity=7)
                break

    mismatches = [i for i in issues if i.kind == "mismatch"]
    return issues, len(mismatches) == 0 and len(issues) == 0


# --- main audit ---
all_issues: list[Issue] = []
ok_count = 0
missing_class = []
missing_file = []

for no in range(1, 105):
    csv = csv_by_no.get(no)
    cls = CLASS_MAP.get(no)
    if not csv or not cls:
        missing_class.append(no)
        continue
    path = class_files.get(cls)
    if not path:
        missing_file.append((no, cls))
        continue
    code = extract_code(path)
    issues, is_ok = compare_card(no, csv, code, cls)
    if is_ok and not issues:
        ok_count += 1
    all_issues.extend(issues)

mismatches = [i for i in all_issues if i.kind == "mismatch"]
inferences = [i for i in all_issues if i.kind == "needs_inference"]

mismatches.sort(key=lambda x: (-x.severity, x.no))
inferences.sort(key=lambda x: x.no)

# Build JSON fixes
fixes_json = []
seen_nos = set()
for i in mismatches + inferences:
    if i.fixes:
        fixes_json.append({"no": i.no, "class": i.cls, "fixes": i.fixes})
        seen_nos.add(i.no)
    elif i.kind == "mismatch":
        fixes_json.append({
            "no": i.no, "class": i.cls,
            "fixes": [{"file": class_files[i.cls].relative_to(ROOT).as_posix() if i.cls in class_files else "",
                       "what": i.suggested[:80], "from": i.code_current[:60], "to": i.suggested[:60], "ug_infer": False}]
        })

# Manual enrichment pass - read all card files for critical checks
# (supplement script with direct reads for known complex cards)

# Write markdown report
lines = [
    "# Hypno Creator UG/Base 数値監査レポート（No.1–104）",
    "",
    f"生成日: 2026-07-19",
    f"データ源: `_csv_cards_1_104.json` + `_card_audit_report.txt`（Class マッピング）",
    "",
    "## サマリー",
    "",
    f"| 区分 | 件数 |",
    f"|------|------|",
    f"| OK（BASE/UG 数値・キーワード一致） | {ok_count} |",
    f"| mismatch（CSV とコード乖離） | {len(mismatches)} |",
    f"| needs_inference（UG 文言曖昧・要手動確認） | {len(inferences)} |",
    f"| クラス未マップ | {len(missing_class)} |",
    f"| ファイル未発見 | {len(missing_file)} |",
    "",
    "> 本監査は CanonicalVars の BASE 値、OnUpgrade の数値差分・キーワード変更（Exhaust/Retain/Innate/コスト）を中心に比較。",
    "> 複雑な挙動変更（AOE 化、山札シャッフル、選択 UI 等）は needs_inference に分類。",
    "",
    "## mismatch 一覧",
    "",
]

if mismatches:
    lines.append("| No | 名称 | Class | CSV BASE（抜粋） | CSV UG | コード現状 | 修正案 |")
    lines.append("|----|------|-------|-------------------|--------|------------|--------|")
    for i in mismatches:
        lines.append(f"| {i.no} | {i.name} | {i.cls} | {i.csv_base[:50]}… | {i.csv_ug[:40]} | {i.code_current} | {i.suggested} |")
else:
    lines.append("_mismatch なし_")

lines.extend(["", "## needs_inference 一覧", ""])
if inferences:
    for i in inferences:
        lines.append(f"### No.{i.no} {i.name}（{i.cls}）")
        lines.append(f"- CSV UG: {i.csv_ug}")
        lines.append(f"- コード: {i.code_current}")
        lines.append(f"- 推定: {i.suggested}")
        lines.append("")
else:
    lines.append("_needs_inference なし_")

lines.extend(["", "## 監査方法", "",
              "1. `_card_audit_report.txt` の `Class:Xxx` で No→クラス特定",
              "2. 各 `.cs` から `CanonicalVars`（DamageVar/BlockVar/DynamicVar/PowerVar）と `OnUpgrade` を抽出",
              "3. CSV `effect`/`ug` から正規表現で数値・キーワードを抽出し比較",
              "4. UG が差分のみ記載の場合は BASE+差分または絶対値を推論（ug_infer フラグ）",
              ""])

OUT_MD.write_text("\n".join(lines), encoding="utf-8")
OUT_JSON.write_text(json.dumps(fixes_json, ensure_ascii=False, indent=2), encoding="utf-8")

print(f"OK={ok_count} mismatch={len(mismatches)} inference={len(inferences)}")
print(f"Report: {OUT_MD}")
print(f"Fixes: {OUT_JSON}")
print("\nTop 20 critical mismatches:")
for i in mismatches[:20]:
    print(f"  [{i.severity}] No.{i.no} {i.name}: {i.code_current} -> {i.suggested}")
