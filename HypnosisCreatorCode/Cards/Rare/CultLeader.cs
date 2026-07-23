using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 教祖化 — パワー。有効な間、アブノーマル／DomSub／SMカードが必ず性癖に刺さる。
/// ターン開始時、ランダムな催眠系カウントカードを手札に加える（UGで2枚）。
/// トランス付与時、1エナジーを得てカードを1枚引く。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CultLeader() : HypnosisCreatorCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cards", 1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<CultLeaderPower>(DynamicVars["Cards"].IntValue)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<CultLeaderPower>(
            choiceContext, Owner.Creature, DynamicVars["Cards"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1M);
}
