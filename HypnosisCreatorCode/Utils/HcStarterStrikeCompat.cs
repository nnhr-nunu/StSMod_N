using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// HC は初期デッキに本家ストライクが無い。ランごとにスターター攻撃3種のいずれかを
/// 「ストライク相当」として扱い、カプセル／ネオーのタリスマン／湿布／ストライクタグ判定を救済する。
/// パンドラの箱は3枚すべてを基本ストライク扱い。ストライクダミー／ヘルレイザーは時止めストライク等の実タグのみ。
/// </summary>
public static class HcStarterStrikeCompat
{
    private static readonly Type[] StarterAttackTypes =
    [
        typeof(StunGun),
        typeof(SayYoureSorry),
        typeof(WristCut)
    ];

    private static readonly ConditionalWeakTable<Player, Type> StandInByPlayer = new();

    public static bool IsHcCharacter(CharacterModel? character) =>
        character?.Id.Entry.Contains(HcCharacter.CharacterId, StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsHcStarterAttack(CardModel card) =>
        card is StunGun or SayYoureSorry or WristCut;

    public static bool IsHcBasicDefend(CardModel card) =>
        card is HcDefend;

    public static Type GetStandInType(Player player)
    {
        if (StandInByPlayer.TryGetValue(player, out var cached))
            return cached;

        var rng = player.RunState.Rng.CombatCardSelection;
        var picked = StarterAttackTypes[rng.NextInt(StarterAttackTypes.Length)];
        StandInByPlayer.Add(player, picked);
        return picked;
    }

    public static CardModel GetStandInPrototype(Player player) =>
        CardFromType(GetStandInType(player));

    private static CardModel CardFromType(Type cardType) =>
        cardType switch
        {
            not null when cardType == typeof(StunGun) => ModelDb.Card<StunGun>(),
            not null when cardType == typeof(SayYoureSorry) => ModelDb.Card<SayYoureSorry>(),
            not null when cardType == typeof(WristCut) => ModelDb.Card<WristCut>(),
            _ => ModelDb.Card<StunGun>()
        };

    public static bool IsStandInStrike(CardModel card)
    {
        if (!IsHcStarterAttack(card))
            return false;

        if (!HypnosisCreatorRunRules.IsHypnosisCreatorActive(card))
            return false;

        if (card.Owner is not { } player)
            return false;

        return card.GetType() == GetStandInType(player);
    }

    public static bool ShouldInjectStrikeTag(CardModel card) => IsStandInStrike(card);

    /// <summary>パンドラの箱・空の檻など <see cref="CardModel.IsBasicStrikeOrDefend"/> 用。攻撃3枚すべて＋防御。</summary>
    public static bool MatchesBasicStrikeOrDefend(CardModel card)
    {
        if (!HypnosisCreatorRunRules.IsHypnosisCreatorActive(card))
            return false;

        if (card.Rarity != CardRarity.Basic)
            return false;

        return IsHcBasicDefend(card) || IsHcStarterAttack(card);
    }

    /// <summary>ストライクダミー等、スターター攻撃3枚をストライク扱いしない効果向け。</summary>
    public static bool IsStrikeDummyExcludedStarter(CardModel? card) =>
        card != null && IsHcStarterAttack(card);

    /// <summary>HC ではヘルレイザーは時止めストライクのみ発動（仮想 Strike のスターターは対象外）。</summary>
    public static bool ShouldSuppressHellraiserEffect(CardModel? card)
    {
        if (card == null || !HypnosisCreatorRunRules.IsHypnosisCreatorActive(card))
            return false;

        return card is not TimeStopStrike;
    }
}
