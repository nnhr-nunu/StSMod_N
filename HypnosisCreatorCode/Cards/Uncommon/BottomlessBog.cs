using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>底なしの沼 — ターン終了時、(トランス+沼)×10破滅（UG15）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BottomlessBog() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DoomPerStack", 10M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BottomlessBogPower>(),
        HoverTipFactory.FromPower<BogPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<BottomlessBogPower>(
            choiceContext, Owner.Creature, DynamicVars["DoomPerStack"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["DoomPerStack"].UpgradeValueBy(5M);
}
