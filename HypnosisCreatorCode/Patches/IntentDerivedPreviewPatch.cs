using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Ancient;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
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

        if (!Mirroring.TryGetIntentAttack(mirroring, target, out var perHit, out var hits))
        {
            CardDamagePreview.SetPreviewPair(__instance, 0M, 0M);
            return;
        }

        CardDamagePreview.SetPreviewPair(__instance, perHit, perHit);
    }
}

/// <summary>
/// ミラーリング — 相手の攻撃意図から連撃数をプレビューに載せる。
/// </summary>
[HarmonyPatch(typeof(RepeatVar), "UpdateCardPreview")]
public static class MirroringRepeatPreviewPatch
{
    public static void Postfix(
        RepeatVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = previewMode;
        _ = runGlobalHooks;
        if (card is not Mirroring mirroring) return;

        if (!Mirroring.TryGetIntentAttack(mirroring, target, out _, out var hits))
        {
            CardDamagePreview.SetPreviewPair(__instance, 0M, 0M);
            return;
        }

        CardDamagePreview.SetPreviewPair(__instance, hits, hits);
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
        if (card is not (HarmonyCard or Agape)) return;

        var previewTarget = target ?? card.CurrentTarget;
        var block = previewTarget != null
            ? EnemyAttackIntents.GetTotalDamage(previewTarget)
            : 0;
        CardDamagePreview.SetPreviewPair(__instance, block, block);
    }
}

/// <summary>
/// ゼロへの近道 — 初回ブロックの敏捷・エンチャント反映を {FirstBlock:diff()} に載せる。
/// </summary>
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.UpdateCardPreview))]
public static class ZeroShortcutFirstBlockPreviewPatch
{
    public static void Postfix(
        DynamicVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = target;
        _ = runGlobalHooks;
        if (__instance.Name != "FirstBlock") return;
        if (card is not ZeroShortcut) return;

        var raw = ZeroShortcut.StartBlock;
        var preview = CardBlockPreview.ApplyModifiers(card, raw, ValueProp.Move, previewMode);
        CardDamagePreview.SetPreviewPair(__instance, raw, preview);
    }
}

/// <summary>
/// ゼロへの近道 — 3→2→1→0 の各 GainBlock に敏捷・エンチャントを反映した合計をプレビューに載せる。
/// </summary>
[HarmonyPatch(typeof(BlockVar), "UpdateCardPreview")]
public static class ZeroShortcutBlockPreviewPatch
{
    public static void Postfix(
        BlockVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = target;
        _ = runGlobalHooks;
        if (card is not ZeroShortcut shortcut) return;

        var baseline = ZeroShortcut.BaselineTotalBlock;
        var preview = ZeroShortcut.ComputePreviewTotalBlock(shortcut, previewMode);
        CardDamagePreview.SetPreviewPair(__instance, baseline, preview);
    }
}
