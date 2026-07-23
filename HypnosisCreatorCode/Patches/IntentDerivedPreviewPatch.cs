using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using HarmonyCard = HypnosisCreator.HypnosisCreatorCode.Cards.Basic.Harmony;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ミラーリング — 相手の攻撃意図からダメージをプレビューに載せる。
/// </summary>
[HarmonyPatch(typeof(DamageVar), "UpdateCardPreview")]
public static class MirroringDamagePreviewPatch
{
    public static void Postfix(
        DamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = runGlobalHooks;
        if (card is not Mirroring mirroring) return;

        var previewTarget = target ?? mirroring.CurrentTarget;
        if (previewTarget == null || !EnemyAttackIntents.TryGetPerHit(previewTarget, out var damage, out _))
        {
            CardDamagePreview.SetPreviewPair(__instance, 0M, 0M);
            return;
        }

        var preview = CardDamagePreview.ApplyModifiers(
            mirroring, previewTarget, damage, ValueProp.Unpowered, previewMode);
        CardDamagePreview.SetPreviewPair(__instance, damage, preview);
    }
}

/// <summary>
/// 調和 — 相手の攻撃合計と同値のブロックをプレビューに載せる。
/// </summary>
[HarmonyPatch(typeof(BlockVar), "UpdateCardPreview")]
public static class HarmonyBlockPreviewPatch
{
    public static void Postfix(
        BlockVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = previewMode;
        _ = runGlobalHooks;
        if (card is not HarmonyCard harmony) return;

        var previewTarget = target ?? harmony.CurrentTarget;
        var block = previewTarget != null
            ? EnemyAttackIntents.GetTotalDamage(previewTarget)
            : 0;
        CardDamagePreview.SetPreviewPair(__instance, block, block);
    }
}
