# -*- coding: utf-8 -*-
"""Generate playable status/curse card sources for CSV 230-255 (excluding already-coded)."""
from pathlib import Path

TOKEN = Path("HypnosisCreatorCode/Cards/Token")

STATUS_HEADER = """using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using HypnosisCreator.HypnosisCreatorCode.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;
"""

CURSE_HEADER = STATUS_HEADER  # same usings; unused trimmed by build? keep broad

files: dict[str, str] = {}

files["SootStatus.cs"] = STATUS_HEADER + """
/// <summary>すす — 状態異常催眠版。廃棄のみ。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SootStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"soot.png\".CardImagePath();
    public override string CustomPortraitPath => \"soot.png\".BigCardImagePath();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        Task.CompletedTask;
}
"""

files["DazedStatus.cs"] = STATUS_HEADER + """
/// <summary>めまい — 一時的な筋力低下1。廃棄。エセリアル。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DazedStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"dazed.png\".CardImagePath();
    public override string CustomPortraitPath => \"dazed.png\".BigCardImagePath();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DazedTempStrengthDownPower>(1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<DazedTempStrengthDownPower>(
            choiceContext, play.Target, DynamicVars[\"DazedTempStrengthDownPower\"].BaseValue,
            Owner.Creature, this);
    }
}
"""

files["VoidStatus.cs"] = STATUS_HEADER + """
/// <summary>虚無 — 1エナジーを得る。廃棄。エセリアル。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class VoidStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"void_status.png\".CardImagePath();
    public override string CustomPortraitPath => \"void_status.png\".BigCardImagePath();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
"""

files["DebrisStatus.cs"] = STATUS_HEADER + """
/// <summary>デブリ — 5ダメージ。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DebrisStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"debris.png\".CardImagePath();
    public override string CustomPortraitPath => \"debris.png\".BigCardImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Unpowered)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, this, play);
    }
}
"""

files["ToxicStatus.cs"] = STATUS_HEADER + """
/// <summary>毒素 — 5ダメージ。廃棄。アブノーマル。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ToxicStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"toxic.png\".CardImagePath();
    public override string CustomPortraitPath => \"toxic.png\".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Unpowered)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, this, play);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
"""

files["BeckonStatus.cs"] = STATUS_HEADER + """
/// <summary>誘い — HP6を失わせる。廃棄。DomSub。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BeckonStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => \"beckon.png\".CardImagePath();
    public override string CustomPortraitPath => \"beckon.png\".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new HpLossVar(6M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature, this, play);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
"""

# FranticEscape — hand-written below as separate careful file
# Curses

def curse(name_cls, portrait, summary, keywords, fetish, body, extra_usings=""):
    kw = keywords
    fetish_line = f"\n    public override IReadOnlyList<FetishType> CardFetishes => [{fetish}];\n" if fetish else ""
    return CURSE_HEADER + extra_usings + f"""
/// <summary>{summary}</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class {name_cls}() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{{
    public override string PortraitPath => \"{portrait}\".CardImagePath();
    public override string CustomPortraitPath => \"{portrait}\".BigCardImagePath();
{fetish_line}
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [{kw}];
{body}
}}
"""

files["AscendersBaneCurse.cs"] = curse(
    "AscendersBaneCurse", "ascenders_bane.png",
    "アセンダーの災厄 — エセリアル。1ドロー。",
    "CardKeyword.Ethereal",
    "",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
""")

files["InjuryCurse.cs"] = curse(
    "InjuryCurse", "injury.png",
    "怪我 — 8ダメージ。廃棄。SM。",
    "CardKeyword.Exhaust",
    "FetishType.Sm",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8M, ValueProp.Unpowered)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, this, play);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["GreedCurse.cs"] = curse(
    "GreedCurse", "greed.png",
    "強欲 — 廃棄。永劫。",
    "CardKeyword.Exhaust, CardKeyword.Eternal",
    "",
    """
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        Task.CompletedTask;
""")

files["DoubtCurse.cs"] = curse(
    "DoubtCurse", "doubt.png",
    "疑念 — 脱力1。廃棄。",
    "CardKeyword.Exhaust",
    "",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
    }
""")

files["WritheCurse.cs"] = curse(
    "WritheCurse", "writhe.png",
    "苦悩 — 天賦。1ドロー。廃棄。SM。",
    "CardKeyword.Innate, CardKeyword.Exhaust",
    "FetishType.Sm",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["FollyCurse.cs"] = curse(
    "FollyCurse", "folly.png",
    "愚行 — 天賦・エセリアル・1ドロー・廃棄・永劫。SM。",
    "CardKeyword.Innate, CardKeyword.Ethereal, CardKeyword.Exhaust, CardKeyword.Eternal",
    "FetishType.Sm",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["RegretCurse.cs"] = curse(
    "RegretCurse", "regret.png",
    "後悔 — 手札1枚につきHP1失わせる。廃棄。アブノーマル。",
    "CardKeyword.Exhaust",
    "FetishType.Abnormal",
    """
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var count = Owner.PlayerCombatState?.Hand?.Cards.Count ?? 0;
        if (count > 0)
        {
            await CreatureCmd.Damage(
                choiceContext, play.Target, count,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                Owner.Creature, this, play);
        }
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["CurseOfTheBellCurse.cs"] = curse(
    "CurseOfTheBellCurse", "curse_of_the_bell.png",
    "鐘の呪い — 廃棄。永劫。",
    "CardKeyword.Exhaust, CardKeyword.Eternal",
    "",
    """
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        Task.CompletedTask;
""")

files["BadLuckCurse.cs"] = curse(
    "BadLuckCurse", "bad_luck.png",
    "不運 — 最大HPの13%分のHPを失わせる。廃棄。",
    "CardKeyword.Exhaust",
    "",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new HpLossVar(13M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var loss = Math.Max(1, (int)Math.Ceiling(play.Target.MaxHp * 0.13));
        await CreatureCmd.Damage(
            choiceContext, play.Target, loss,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature, this, play);
    }
""")

files["ClumsyCurse.cs"] = curse(
    "ClumsyCurse", "clumsy.png",
    "不器用 — エセリアル。敏捷低下2。廃棄。",
    "CardKeyword.Ethereal, CardKeyword.Exhaust",
    "",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ClumsyTempDexterityDownPower>(2M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<DexterityPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<ClumsyTempDexterityDownPower>(
            choiceContext, play.Target, DynamicVars[\"ClumsyTempDexterityDownPower\"].BaseValue,
            Owner.Creature, this);
    }
""")

files["DecayCurse.cs"] = curse(
    "DecayCurse", "decay.png",
    "腐敗 — 2ダメージ。廃棄。アブノーマル。",
    "CardKeyword.Exhaust",
    "FetishType.Abnormal",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2M, ValueProp.Unpowered)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, this, play);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["DebtCurse.cs"] = curse(
    "DebtCurse", "debt.png",
    "負債 — 戦闘終了後に追加10ゴールド。廃棄。",
    "CardKeyword.Exhaust",
    "",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new GoldVar(10)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ProselytizeRewards.AddGold(Owner, DynamicVars.Gold.BaseValue);
        return Task.CompletedTask;
    }
""")

files["NormalityCurse.cs"] = curse(
    "NormalityCurse", "normality.png",
    "凡庸 — スロー1。廃棄。DomSub。",
    "CardKeyword.Exhaust",
    "FetishType.DomSub",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<SlowPower>(1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<SlowPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<SlowPower>(
            choiceContext, play.Target, DynamicVars[\"SlowPower\"].BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["ShameCurse.cs"] = curse(
    "ShameCurse", "shame.png",
    "羞恥 — 弱体1。廃棄。DomSub。",
    "CardKeyword.Exhaust",
    "FetishType.DomSub",
    """
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<VulnerablePower>(1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, play.Target, DynamicVars[\"VulnerablePower\"].BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["SporeMindCurse.cs"] = curse(
    "SporeMindCurse", "spore_mind.png",
    "菌糸の汚染 — 廃棄。アブノーマル。",
    "CardKeyword.Exhaust",
    "FetishType.Abnormal",
    """
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

files["EnthralledCurse.cs"] = curse(
    "EnthralledCurse", "enthralled.png",
    "心酔 — 必ず性癖に刺さる。廃棄。永劫。全性癖。",
    "CardKeyword.Exhaust, CardKeyword.Eternal",
    "FetishType.DomSub, FetishType.Sm, FetishType.Abnormal",
    """
    public override bool AlwaysHitsFetish => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ResolveFetishOnTarget(choiceContext, play);
    }
""")

for name, content in files.items():
    # fix escaped quotes from generator
    content = content.replace('\\"', '"')
    (TOKEN / name).write_text(content, encoding="utf-8")
    print("wrote", name)

print("count", len(files))
