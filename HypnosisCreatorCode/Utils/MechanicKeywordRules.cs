using System.Collections.Concurrent;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 説明文の [gold]トランス[/gold]／破滅／沼 に inline キーワードツールチップを付ける判定。
/// 性癖タグの <see cref="HcKeywords.Trance"/>（タイトル「トランス性癖」）とは別。
/// </summary>
public static class MechanicKeywordRules
{
    private const string CardsLocTable = "cards";

    private static readonly ConcurrentDictionary<string, MechanicKeywordFlags> FlagsCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, string?> DescriptionCache = new(StringComparer.Ordinal);

    public static IEnumerable<CardKeyword> KeywordsFor(CardModel card)
    {
        var flags = GetMechanicFlags(card);
        if (flags.Trance)
            yield return HcKeywords.TranceState;
        if (flags.Doom)
            yield return HcKeywords.Doom;
        if (flags.Bog)
            yield return HcKeywords.Bog;
    }

    private static MechanicKeywordFlags GetMechanicFlags(CardModel card)
    {
        if (KeywordPatchGuard.IsNested || card.IsCanonical)
            return ComputeMechanicFlagsVarsOnly(card);

        var entry = card.Id?.Entry;
        if (string.IsNullOrEmpty(entry))
            return ComputeMechanicFlagsVarsOnly(card);

        if (FlagsCache.TryGetValue(entry, out var cached))
            return cached;

        var flags = ComputeMechanicFlags(card, entry);
        FlagsCache.TryAdd(entry, flags);
        return flags;
    }

    private static MechanicKeywordFlags ComputeMechanicFlags(CardModel card, string entry)
    {
        var flags = ComputeMechanicFlagsVarsOnly(card);

        if (flags.Trance && flags.Doom && flags.Bog)
            return flags;

        var description = GetCardDescriptionText(card, entry);
        if (description is null)
            return flags;

        if (!flags.Trance
            && (description.Contains("[gold]トランス[/gold]", StringComparison.Ordinal)
                || description.Contains("[gold]Trance[/gold]", StringComparison.Ordinal)))
            flags.Trance = true;

        if (!flags.Doom
            && (description.Contains("[gold]破滅[/gold]", StringComparison.Ordinal)
                || description.Contains("[gold]Doom[/gold]", StringComparison.Ordinal)))
            flags.Doom = true;

        if (!flags.Bog
            && (description.Contains("[gold]沼[/gold]", StringComparison.Ordinal)
                || description.Contains("[gold]Bog[/gold]", StringComparison.Ordinal)))
            flags.Bog = true;

        return flags;
    }

    private static MechanicKeywordFlags ComputeMechanicFlagsVarsOnly(CardModel card) =>
        new()
        {
            Trance = HasDynamicVar(card, "Trance"),
            Doom = HasDynamicVar(card, "Doom"),
            Bog = HasDynamicVar(card, "Bog")
        };

    private static bool HasDynamicVar(CardModel card, string name) =>
        card.DynamicVars?.Values.Any(v => v.Name == name) == true;

    private static string? GetCardDescriptionText(CardModel card, string entryId)
    {
        if (DescriptionCache.TryGetValue(entryId, out var cached))
            return cached;

        string? text = null;
        if (!KeywordPatchGuard.IsNested)
        {
            try
            {
                var loc = new LocString(CardsLocTable, $"{entryId}.description");
                card.DynamicVars.AddTo(loc);
                text = loc.GetFormattedText();
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"Mechanic keyword description lookup failed for {entryId}: {ex.Message}");
                text = null;
            }
        }

        DescriptionCache.TryAdd(entryId, text);
        return text;
    }

    private struct MechanicKeywordFlags
    {
        public bool Trance;
        public bool Doom;
        public bool Bog;
    }
}
