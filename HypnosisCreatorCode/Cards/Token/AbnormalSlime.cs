using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 粘液 — 状態異常。アブノーマル目覚め＋1ドロー。廃棄。
/// 本家 Slimed の状態異常催眠置き換え先／スライムバーサーカー付与。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalSlime() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        FetishCombat.Awaken(play.Target, FetishType.Abnormal, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
}
