using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// ラポール — パワー。プレイ時に手札カウントを1進める。
/// ターン開始時、前ターン未攻撃なら追加で1進める（UGで無条件）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Rapport() : HypnosisCreatorCard(0,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Stacks", 1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<RapportPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CountRules.AdvanceHandCountCards(Owner);
        await PowerCmd.Apply<RapportPower>(
            choiceContext, Owner.Creature, DynamicVars["Stacks"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Stacks"].UpgradeValueBy(1M);
}
