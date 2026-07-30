# -*- coding: utf-8 -*-
"""CSV No.111-187 vs implemented hearts — build a work plan."""
import json
import re
from pathlib import Path

csv_rows = json.loads(Path("_hearts_csv_111_187.json").read_text(encoding="utf-8"))

# Known mapping: Japanese name fragment -> class (from existing hearts)
# We'll also scan all heart .cs files for IsRareHeart and effect lines.

hearts_dir = Path("HypnosisCreatorCode/Relics/Hearts")
impl = {}
for p in hearts_dir.glob("*Heart.cs"):
    if p.name in ("EnemyHeartRelic.cs", "StolenHeart.cs", "HeartActivationHelpers.cs"):
        continue
    text = p.read_text(encoding="utf-8")
    m_rare = re.search(r"IsRareHeart\s*=>\s*(true|false)", text)
    m_id = re.search(r'MonsterIdEntry\s*=>\s*"([^"]+)"', text)
    # rough effect
    act = ""
    if "OnPassiveObtain" in text or "PassiveGold" in text or "PassiveMaxHp" in text:
        act = "PASSIVE"
    if "ActivateAsync" in text:
        # grab await line(s)
        awaits = re.findall(r"await\s+([^\n;]+)", text)
        act = " | ".join(awaits[:3]) if awaits else act or "CUSTOM"
    impl[p.stem] = {
        "file": p.name,
        "rare": m_rare.group(1) if m_rare else "?",
        "monster_id": m_id.group(1) if m_id else "?",
        "effect": act[:200],
    }

# Print CSV effects for planning
lines = []
lines.append(f"CSV hearts: {len(csv_rows)}")
lines.append(f"Impl hearts: {len(impl)}")
lines.append("")
for r in csv_rows:
    no = r["No"]
    name = r["カード名称（日本語）"]
    eff = (r.get("効果説明") or "").replace("\n", " ")
    note = (r.get("備考") or "").replace("\n", " / ")
    lines.append(f"{no}\t{name}\t{eff}")
    if note.strip():
        lines.append(f"\tNOTE: {note}")

lines.append("\n=== NON-RARE IMPL (must convert) ===")
for k, v in sorted(impl.items()):
    if v["rare"] == "false":
        lines.append(f"{k}\t{v['monster_id']}\t{v['effect']}")

Path("_hearts_audit_plan.txt").write_text("\n".join(lines), encoding="utf-8")
print("wrote _hearts_audit_plan.txt")
print("non-rare count:", sum(1 for v in impl.values() if v["rare"] == "false"))
print("csv count:", len(csv_rows))
