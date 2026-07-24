using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘時括弧プレビューを1箇所に集約。カード static ctor からの多重登録と、
/// コレクション表示時の未捕捉例外（"there is a bug"）を防ぐ。
/// </summary>
public static class CombatSuffixDescriptionHooks
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += ApplySafely;

    private static void ApplySafely(CardModel card, Creature? target, ref string description)
    {
        try
        {
            Apply(card, target, ref description);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Combat suffix description failed for {card.Id}: {ex.Message}");
        }
    }

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        switch (card)
        {
            case MeltIntoTrance melt:
                MeltIntoTrance.AppendDescriptionSuffix(melt, target, ref description);
                break;
            case MeetExpectations meet:
                MeetExpectations.AppendDescriptionSuffix(meet, target, ref description);
                break;
            case Autopsy autopsy:
                Autopsy.AppendDescriptionSuffix(autopsy, target, ref description);
                break;
            case VitalPoint vital:
                VitalPoint.AppendDescriptionSuffix(vital, target, ref description);
                break;
            case PerversionTrueDesire desire:
                PerversionTrueDesire.AppendDescriptionSuffix(desire, target, ref description);
                break;
            case ZeroShortcut shortcut:
                ZeroShortcut.AppendDescriptionSuffix(shortcut, target, ref description);
                break;
            case InfiniteFingerSnap snap:
                InfiniteFingerSnap.AppendDescriptionSuffix(snap, target, ref description);
                break;
            case Punishment punishment:
                Punishment.AppendDescriptionSuffix(punishment, target, ref description);
                break;
            case FetishChampion champion:
                FetishChampion.AppendDescriptionSuffix(champion, target, ref description);
                break;
            case HypnosisIntro intro:
                HypnosisIntro.AppendDescriptionSuffix(intro, target, ref description);
                break;
            case AmbushHypnosis ambush:
                AmbushHypnosis.AppendDescriptionSuffix(ambush, target, ref description);
                break;
            case SuggestionRelease release:
                SuggestionRelease.AppendDescriptionSuffix(release, target, ref description);
                break;
        }
    }
}
