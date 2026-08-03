using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心臓への渇望 — 保有心臓数×無慈悲25（本家 Cruelty 同値）。UGで残虐2（本家 ViciousPower）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartCraving() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    /// <summary>本家 Cruelty／腹部への殴打 UG と同じ1心臓あたりの付与量。</summary>
    private const decimal CrueltyPerHeart = 25M;

    /// <summary>本家アイアンクラッド「Vicious」カードと同じ付与量（心臓数とは無関係）。</summary>
    private const decimal ViciousAmount = 2M;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<CrueltyPower>(CrueltyPerHeart),
        new PowerVar<ViciousPower>(ViciousAmount)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<CrueltyPower>();
            if (IsUpgraded)
                yield return HoverTipFactory.FromPower<ViciousPower>();
        }
    }

    protected override bool ShouldGlowWhenConditionMet() =>
        HeartInventory.CountHearts(Owner) > 0 || IsUpgraded;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var cruelty = CrueltyPerHeart * HeartInventory.CountHearts(Owner);
        if (cruelty > 0)
        {
            await PowerCmd.Apply<CrueltyPower>(
                choiceContext, Owner.Creature, cruelty, Owner.Creature, this);
        }

        if (IsUpgraded)
        {
            await PowerCmd.Apply<ViciousPower>(
                choiceContext, Owner.Creature, ViciousAmount, Owner.Creature, this);
        }

        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 残虐2追加分は upgradeAppend 説明。コスト据え置き。
    }
}
