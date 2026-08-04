using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 心臓入手リーサル（ダメージとどめ）の共通判定・🫀🗡️ サフィックス。
/// </summary>
public static class HeartCaptureLethalPreview
{
    public static bool WouldDamageKill(
        CardModel card,
        Creature target,
        decimal rawDamage,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (!IsValidLethalTarget(target)) return false;
        if (!CombatPreviewText.IsActive(card)) return false;
        if (rawDamage <= 0) return false;

        var effective = CardDamagePreview.ApplyModifiers(
            card, target, rawDamage, props, previewMode);
        var hpLoss = CombatHpLossPreview.ComputeHpLossFromAttackDamage(
            card, target, effective, props);
        return CombatHpLossPreview.WouldDamageKill(target, hpLoss);
    }

    public static bool TryGetDamageLethalSuffix(
        CardModel card,
        Creature? target,
        decimal rawDamage,
        ValueProp props,
        CardPreviewMode previewMode,
        out string suffix)
    {
        suffix = "";
        if (!CombatPreviewText.IsActive(card)) return false;

        var previewTarget = ResolvePreviewTarget(card, target);
        if (previewTarget == null) return false;
        if (!WouldDamageKill(card, previewTarget, rawDamage, props, previewMode)) return false;

        suffix = UpgradeCardText.IsJapaneseUi()
            ? "（🫀🗡️ リーサル）"
            : " (🫀🗡️ Lethal)";
        return true;
    }

    public static void TryAppendDamageLethalSuffix(
        CardModel card,
        Creature? target,
        decimal rawDamage,
        ValueProp props,
        ref string description,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (!TryGetDamageLethalSuffix(card, target, rawDamage, props, previewMode, out var suffix)) return;
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    internal static bool IsValidLethalTarget(Creature? target) =>
        target is { IsAlive: true, IsEnemy: true };

    internal static Creature? ResolvePreviewTarget(CardModel card, Creature? target) =>
        target is { IsAlive: true, IsEnemy: true } ? target
        : card.CurrentTarget is { IsAlive: true, IsEnemy: true } ? card.CurrentTarget
        : null;
}
