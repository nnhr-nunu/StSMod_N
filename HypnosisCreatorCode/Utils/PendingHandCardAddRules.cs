using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カードプレイ中の手札遅延追加は mod カードのみ対象。本家キャラ効果（リージェントの Forge 等）を巻き込まない。
/// </summary>
internal static class PendingHandCardAddRules
{
    public static bool ShouldDeferGeneratedHandAdd(IReadOnlyList<CardModel> cards) =>
        cards.Count > 0 && cards.All(IsModCard);

    public static bool ShouldDeferExistingHandAdd(CardModel card) => IsModCard(card);

    private static bool IsModCard(CardModel card) => card.Pool is HypnosisCreatorCardPool;
}
