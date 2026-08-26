using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>戦闘中のランダムカード生成向け。本家 <see cref="CardPoolModel.GetUnlockedCards"/> のマルチ制約と同趣旨。</summary>
public static class CombatCardGenerationRules
{
    public static bool MatchesRunMultiplayerConstraint(
        CardModel card,
        CardMultiplayerConstraint runConstraint) =>
        card.MultiplayerConstraint == CardMultiplayerConstraint.None ||
        card.MultiplayerConstraint == runConstraint;
}
