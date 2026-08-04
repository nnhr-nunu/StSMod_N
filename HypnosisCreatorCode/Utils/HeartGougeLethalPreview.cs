using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 心臓えぐり出しのリーサル補助プレビュー（破滅とどめ＋ダメージリーサル）。
/// </summary>
public static class HeartGougeLethalPreview
{
    public static bool WouldDoomExecuteHeartCapture(HeartGouge card, Creature target) =>
        card.IsUpgraded && HeartGouge.CanDoomExecute(target);

    public static bool WouldDamageKillHeartCapture(
        HeartGouge card,
        Creature target,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (!HeartCaptureLethalPreview.IsValidLethalTarget(target)) return false;
        if (!CombatPreviewText.IsActive(card)) return false;

        var effectiveDamage = HeartGouge.ComputeDamagePreview(
            card, target, previewMode, runGlobalHooks: true);
        var hpLoss = CombatHpLossPreview.ComputeHpLossFromAttackDamage(
            card, target, effectiveDamage, ValueProp.Move);
        return CombatHpLossPreview.WouldDamageKill(target, hpLoss);
    }

    public static bool WouldShowHeartLethal(
        HeartGouge card,
        Creature target,
        CardPreviewMode previewMode = CardPreviewMode.Normal) =>
        HeartCaptureLethalPreview.IsValidLethalTarget(target)
        && (WouldDoomExecuteHeartCapture(card, target)
            || WouldDamageKillHeartCapture(card, target, previewMode));

    public static bool TryGetLethalSuffix(
        HeartGouge card,
        Creature? target,
        CardPreviewMode previewMode,
        out string suffix)
    {
        suffix = "";
        if (!CombatPreviewText.IsActive(card)) return false;

        var previewTarget = HeartCaptureLethalPreview.ResolvePreviewTarget(card, target);
        if (previewTarget == null) return false;

        if (WouldDoomExecuteHeartCapture(card, previewTarget))
        {
            suffix = UpgradeCardText.IsJapaneseUi()
                ? "（🫀🗡️ 破滅とどめ）"
                : " (🫀🗡️ Doom execute)";
            return true;
        }

        if (WouldDamageKillHeartCapture(card, previewTarget, previewMode))
        {
            suffix = UpgradeCardText.IsJapaneseUi()
                ? "（🫀🗡️ リーサル）"
                : " (🫀🗡️ Lethal)";
            return true;
        }

        return false;
    }

    internal static void AppendDescriptionSuffix(
        HeartGouge card,
        Creature? target,
        ref string description)
    {
        if (!TryGetLethalSuffix(card, target, CardPreviewMode.Normal, out var suffix)) return;
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }
}
