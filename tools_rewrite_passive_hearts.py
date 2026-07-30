# -*- coding: utf-8 -*-
"""Rewrite former passive hearts + sync loc descriptions from CSV 111-187."""
import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
csv_rows = json.loads((ROOT / "_hearts_csv_111_187.json").read_text(encoding="utf-8"))

# Japanese CSV name (short) -> (class_stem, monster_id, activate_snippet)
# activate_snippet is C# body for ActivateAsync (without signature)

PASSIVE_TO_RARE = {
    "LeafSlimeMedHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リーフスライム(中)の心臓 — 希少。最大HP+2。</summary>
public class LeafSlimeMedHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LEAF_SLIME_M";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 2);
}
''',
    "TwigSlimeMedHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>細枝スライム(中)の心臓 — 希少。最大HP+2。</summary>
public class TwigSlimeMedHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TWIG_SLIME_M";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 2);
}
''',
    "FogMogHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>フォグモグの心臓 — 希少。最大HP+4。</summary>
public class FogMogHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FOGMOG";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 4);
}
''',
    "VineShamblerHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ヴァインシャンブラーの心臓 — 希少。25ゴールド。</summary>
public class VineShamblerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "VINE_SHAMBLER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
''',
    "RitualBeastHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>儀式の獣の心臓 — 希少。最大HP+10。</summary>
public class RitualBeastHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "CEREMONIAL_BEAST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 10);
}
''',
    "BloodPriestHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>血族の司祭の心臓 — 希少。最大HP+5、ゴールド50。</summary>
public class BloodPriestHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "KIN_PRIEST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareMaxHpAndGold(this, choiceContext, player, 5, 50);
}
''',
    "GhostShipHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>幽霊船の心臓 — 希少。55ゴールド。</summary>
public class GhostShipHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "HAUNTED_SHIP";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 55);
}
''',
    "TwinTailRatHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>双尾のネズミの心臓 — 希少。25ゴールド。</summary>
public class TwinTailRatHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TWO_TAILED_RAT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
''',
    "WaterfallGiantHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>滝の巨人の心臓 — 希少。100ゴールド。</summary>
public class WaterfallGiantHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "WATERFALL_GIANT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 100);
}
''',
    "ThiefGrasshopperHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>泥棒バッタの心臓 — 希少。50ゴールド。</summary>
public class ThiefGrasshopperHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THIEVING_HOPPER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 50);
}
''',
    "FatGremlinHeart": '''using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ファットグレムリンの心臓 — 希少。25ゴールド。</summary>
public class FatGremlinHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FAT_GREMLIN";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
''',
}

hearts = ROOT / "HypnosisCreatorCode/Relics/Hearts"
for name, content in PASSIVE_TO_RARE.items():
    (hearts / f"{name}.cs").write_text(content, encoding="utf-8", newline="\n")
    print("rewrote", name)

# Strip redundant `public override bool IsRareHeart => true;` from all hearts (optional cleanup)
for p in hearts.glob("*Heart.cs"):
    if p.name in ("EnemyHeartRelic.cs", "StolenHeart.cs"):
        continue
    text = p.read_text(encoding="utf-8")
    new = re.sub(r"\r?\n\s*public override bool IsRareHeart => true;\r?\n", "\n", text)
    if new != text:
        p.write_text(new, encoding="utf-8", newline="\n")
        print("stripped IsRareHeart", p.name)

print("done rewrites")
