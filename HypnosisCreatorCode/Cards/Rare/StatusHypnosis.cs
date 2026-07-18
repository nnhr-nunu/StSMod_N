using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 状態異常催眠 — パワー。CSV: トランス中の敵へ状態異常・呪いを付与しやすくする効果を想定。
/// 個別の状態異常・呪い付与カードは未確定のため、暫定でマーカーパワーのみ付与する。UGでInnate。
/// TODO: mechanics-lock.md「後送」の状態異常・呪い個別効果が確定したら実装する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class StatusHypnosis() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<StatusHypnosisPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
