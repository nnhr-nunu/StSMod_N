using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Ancient;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
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
/// 時止めストライク — ターン終了ダメージの弱体・感度3000倍等を {Damage:diff()} に反映。
/// </summary>
[HarmonyPatch(typeof(DamageVar), "UpdateCardPreview")]
public static class TimeStopStrikeDamagePreviewPatch
{
    public static void Postfix(
        DamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = runGlobalHooks;
        if (card is not TimeStopStrike strike) return;

        var raw = strike.DynamicVars.Damage.BaseValue;
        var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(strike, raw, ValueProp.Move);
        var preview = TimeStopStrikeDamage.ResolvePreview(
            strike, target ?? strike.CurrentTarget, enchanted, previewMode, runGlobalHooks);
        CardDamagePreview.SetDamagePreviewPair(strike, __instance, raw, preview, ValueProp.Move);
    }
}

/// <summary>
/// 心臓えぐり出し — 先付与の弱体と無慈悲を {Damage:diff()} に反映（アーティファクト時は例外）。
/// </summary>
[HarmonyPatch(typeof(DamageVar), "UpdateCardPreview")]
public static class HeartGougeDamagePreviewPatch
{
    public static void Postfix(
        DamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        if (card is not HeartGouge gouge) return;

        var raw = gouge.DynamicVars.Damage.BaseValue;
        var props = __instance.Props;

        // エンチャント付与プレビューは戦闘フック・先付与弱体より蝕み等の倍率だけ見せる
        if (gouge.IsEnchantmentPreview)
        {
            var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(gouge, raw, props);
            CardDamagePreview.SetDamagePreviewPair(gouge, __instance, raw, enchanted, props);
            return;
        }

        var preview = HeartGouge.ComputeDamagePreview(gouge, target, previewMode, runGlobalHooks);
        CardDamagePreview.SetDamagePreviewPair(gouge, __instance, raw, preview, props);
    }
}

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

        var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(mirroring, perHit, __instance.Props);
        CardDamagePreview.SetDamagePreviewPair(mirroring, __instance, perHit, enchanted, __instance.Props);
    }
}

/// <summary>
/// ミラーリング — 相手の攻撃意図から連撃数をプレビューに載せる。
/// RepeatVar は UpdateCardPreview を継承しないため DynamicVar をパッチする。
/// </summary>
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.UpdateCardPreview))]
public static class MirroringRepeatPreviewPatch
{
    public static void Postfix(
        DynamicVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = previewMode;
        _ = runGlobalHooks;
        if (__instance is not RepeatVar) return;
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
        CardBlockPreview.SetBlockPreviewPair(card, __instance, block, block, __instance.Props);
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
        var enchanted = CardBlockPreview.ApplyEnchantmentModifiers(card, raw, ValueProp.Move);
        var preview = CardBlockPreview.ApplyModifiers(
            card, enchanted, ValueProp.Move, previewMode, runGlobalHooks);
        CardBlockPreview.SetBlockPreviewPair(card, __instance, raw, preview, ValueProp.Move);
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
        var preview = ZeroShortcut.ComputePreviewTotalBlock(shortcut, previewMode, runGlobalHooks);
        CardBlockPreview.SetBlockPreviewPair(shortcut, __instance, baseline, preview, __instance.Props);
    }
}
