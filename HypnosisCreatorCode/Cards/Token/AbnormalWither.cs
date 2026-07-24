using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 衰微 — 状態異常。衰微の予兆スタックを6に戻し、1ドロー。廃棄。
/// 本家 Wither の状態異常催眠置き換え先／永劫の砂時計付与。
/// 備考: 使用前・使用後の両方で6に設定する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalWither() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        // 使用前
        WitherOmen.ResetOn(Owner.Creature);
        WitherOmen.ResetOn(play.Target);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        // 使用後（AfterCardPlayed の減算前に一度戻す。最終確定は Late / パッチ側）
        WitherOmen.ResetOn(Owner.Creature);
        WitherOmen.ResetOn(play.Target);
    }
}
