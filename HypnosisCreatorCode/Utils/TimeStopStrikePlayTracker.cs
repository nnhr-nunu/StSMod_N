using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 時止めストライクのターン内プレイ回数。Mutable コピー間は <see cref="CardModel.CanonicalInstance"/> で共有する。
/// </summary>
internal static class TimeStopStrikePlayTracker
{
    private static readonly ConditionalWeakTable<CardModel, PerTurnCounter> Counters = new();

    public static PerTurnCounter Get(CardModel card) =>
        Counters.GetValue(card.CanonicalInstance ?? card, _ => new PerTurnCounter());
}
