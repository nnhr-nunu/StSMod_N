using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>感度3000倍 —「必ず性癖に刺さる」を1文字レインボー表示。</summary>
public static class Sensitivity3000Description
{
    private const string PlainJa = "必ず性癖に刺さる";
    private static readonly string RainbowJa = RainbowText.JapaneseAlwaysHitsFetish;

    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        _ = target;
        if (card is not Sensitivity3000) return;
        if (!UpgradeCardText.IsJapaneseUi()) return;
        if (description.Contains(RainbowJa, StringComparison.Ordinal)) return;
        if (!description.Contains(PlainJa, StringComparison.Ordinal)) return;

        description = description.Replace(PlainJa, RainbowJa, StringComparison.Ordinal);
    }
}
