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
        // イベントホバー等の正規テンプレは Owner 参照で CanonicalModelException になる。
        if (card is { IsCanonical: true })
            return false;

        // 戦闘中の生成カード（ナイフ等）は Owner が最も信頼できる。RunState.Players だけだと取りこぼす。
        if (card?.Owner != null && IsHypnosisCreator(card.Owner))
            return true;

        if (card?.Owner?.RunState is { } ownerRun && HasHypnosisCreator(ownerRun))
            return true;

        if (card?.CombatState is { } combat && combat.Players.Any(IsHypnosisCreator))
            return true;

        return false;
    }

    private static bool HasHypnosisCreator(IRunState runState) =>
        runState.Players.Any(IsHypnosisCreator);
}
