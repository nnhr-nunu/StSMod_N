# -*- coding: utf-8 -*-
"""Add PreviewDamage/PreviewBlock/PreviewHits to heart relic classes."""
from pathlib import Path
import re

ROOT = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreatorCode\Relics\Hearts")

# class -> (damage, hits, block, per_heart)
SPEC = {
    "AssassinRaiderHeart": (10, 1, 0, False),
    "CrossbowRaiderHeart": (14, 1, 0, False),
    "SneakyGremlinHeart": (9, 1, 0, False),
    "ObscuraHeart": (16, 1, 0, False),
    "SeapunkHeart": (2, 4, 0, False),
    "HunterKillerHeart": (7, 3, 0, False),
    "EntmancerHeart": (3, 7, 0, False),
    "TurretOperatorHeart": (2, 5, 0, False),
    "NibbitHeart": (0, 1, 5, False),
    "AxeRaiderHeart": (0, 1, 5, False),
    "BowlBugEggHeart": (0, 1, 7, False),
    "ProgenitorBugHeart": (0, 1, 14, False),
    "TunnelorHeart": (0, 1, 32, False),
    "OvicopterHeart": (0, 1, 2, True),
    "GlobeHeadHeart": (0, 1, 6, False),
}

for cls, (dmg, hits, block, per_heart) in SPEC.items():
    path = ROOT / f"{cls}.cs"
    text = path.read_text(encoding="utf-8")
    if "PreviewDamage" in text or "PreviewBlock" in text:
        print("skip", cls)
        continue

    overrides = []
    if dmg > 0:
        overrides.append(f"    protected override decimal PreviewDamage => {dmg};")
        if hits > 1:
            overrides.append(f"    protected override int PreviewHits => {hits};")
    if block > 0 or per_heart:
        overrides.append(f"    protected override decimal PreviewBlock => {block if block else 2};")
        if per_heart:
            overrides.append("    protected override bool PreviewBlockPerOwnedHeart => true;")

    insert = "\n".join(overrides) + "\n\n"
    # After MonsterIdEntry line (or MonsterIdEntries block)
    m = re.search(r'(public override string MonsterIdEntry => "[^"]+";\n)', text)
    if not m:
        print("NO ID", cls)
        continue
    text = text[: m.end()] + "\n" + insert + text[m.end() :]

    # Prefer DynamicVars for activate amounts
    if dmg > 0:
        if hits > 1:
            text = re.sub(
                rf"ActivateRareRandomEnemyDamage\(this, choiceContext, player, {dmg}, {hits}\)",
                f"ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue, {hits})",
                text,
            )
        else:
            text = re.sub(
                rf"ActivateRareRandomEnemyDamage\(this, choiceContext, player, {dmg}\)",
                "ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue)",
                text,
            )
        if "DynamicVars" in text and "Localization.DynamicVars" not in text and "using MegaCrit.Sts2.Core.Localization.DynamicVars" not in text:
            # DynamicVars is on RelicModel - no extra using needed for property access
            pass
    if block > 0 and not per_heart:
        text = re.sub(
            rf"ActivateRareSelfBlock\(this, choiceContext, player, {block}\)",
            "ActivateRareSelfBlock(this, choiceContext, player, DynamicVars.Block.BaseValue)",
            text,
        )

    path.write_text(text, encoding="utf-8", newline="\n")
    print("updated", cls)
