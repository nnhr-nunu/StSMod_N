using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心臓への渇望 — 使用済みの希少心臓をこの戦闘中だけ再使用可能に。
/// 未使用の心臓は触らない。戦闘終了後、使った心臓は使用済みに戻る。
/// UG: 再使用可能フラグが戦闘後も残る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartCraving() : HypnosisCreatorCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (IsUpgraded)
            HeartInventory.RefreshAllPermanently(Owner);
        else
            HeartInventory.RefreshAllForCombat(Owner);

        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 効果差し替えのみ（コスト据え置き）。CSV: UGで永続再使用。
    }
}
