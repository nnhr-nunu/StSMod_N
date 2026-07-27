using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>ヒプノクリエイターがラン／戦闘に参加しているかの判定。</summary>
public static class HypnosisCreatorRunRules
{
    public static bool IsHypnosisCreator(Player? player) =>
        player?.Character?.Id.Entry.Contains(
            HcCharacter.CharacterId, StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// ソロで HC をプレイ中、またはマルチで HC がパーティにいるときだけ true。
    /// カード一覧などラン外（Owner／CombatState なし）は false。
    /// </summary>
    public static bool IsHypnosisCreatorActive(CardModel? card = null)
    {
        if (card?.Owner?.RunState is { } ownerRun)
            return HasHypnosisCreator(ownerRun);

        if (card?.CombatState is { } combat)
            return combat.Players.Any(IsHypnosisCreator);

        return false;
    }

    private static bool HasHypnosisCreator(IRunState runState) =>
        runState.Players.Any(IsHypnosisCreator);
}
