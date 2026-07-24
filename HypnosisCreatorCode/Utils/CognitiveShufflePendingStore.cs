using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル選択結果。OnPlay のカード実体と AfterCardPlayed の <c>play.Card</c> が
/// 一致しないことがあるため、<see cref="Player"/> 単位で保持する。
/// </summary>
public static class CognitiveShufflePendingStore
{
    public sealed class Pending
    {
        public Type FormType = null!;
        public CharacterModel? Disguise;
        public Creature? TranceTarget;
        public decimal CardsAmount;
        public CardModel? FormCanonical;
        public bool Active;
    }

    private static readonly ConditionalWeakTable<Player, Pending> Table = new();

    public static void Set(
        Player player,
        Type formType,
        CharacterModel? disguise,
        Creature? tranceTarget,
        decimal cardsAmount,
        CardModel? formCanonical)
    {
        var pending = Table.GetValue(player, static _ => new Pending());
        pending.FormType = formType;
        pending.Disguise = disguise;
        pending.TranceTarget = tranceTarget;
        pending.CardsAmount = cardsAmount;
        pending.FormCanonical = formCanonical;
        pending.Active = true;
    }

    public static bool HasPending(Player player) =>
        Table.TryGetValue(player, out var state) && state.Active;

    public static bool TryTake(Player player, out Pending pending)
    {
        pending = null!;
        if (!Table.TryGetValue(player, out var state) || !state.Active)
            return false;
        state.Active = false;
        pending = state;
        return true;
    }
}
