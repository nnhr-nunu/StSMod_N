using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Potions;

/// <summary>
/// クロロホルムポーション — 専用ポーション。対象にトランス1と睡眠1を付与する。
/// 睡眠の見た目はポリネシアン催眠／寝かしつけ催眠と同じ <see cref="ForcedSleep"/>。
/// </summary>
public class ChloroformPotion : HypnosisCreatorPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 1M),
        new PowerVar<AsleepPower>(1M)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<AsleepPower>()];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var applier = Owner.Creature;

        await TranceCombat.ApplyTrance(
            choiceContext, target, DynamicVars["Trance"].IntValue, applier, cardSource: null);
        await PowerCmd.Apply<AsleepPower>(
            choiceContext, target, DynamicVars["AsleepPower"].BaseValue, applier, null);
        await ForcedSleep.EnsurePresentation(choiceContext, target, applier, cardSource: null);
    }
}
