using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="NCard"/> のコスト斜線（_unplayableEnergyIcon / _unplayableStarIcon）を少し透過させ、
/// コスト数値の視認性を上げる。
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
public static class UnplayableEnergySlashPatch
{
    public static void Postfix(NCard __instance) =>
        UnplayableCostSlashVisual.Apply(__instance, UnplayableCostSlashVisual.EnergyIconField);
}

[HarmonyPatch(typeof(NCard), "UpdateStarCostVisuals")]
public static class UnplayableStarSlashPatch
{
    public static void Postfix(NCard __instance) =>
        UnplayableCostSlashVisual.Apply(__instance, UnplayableCostSlashVisual.StarIconField);
}

internal static class UnplayableCostSlashVisual
{
    /// <summary>本家は不透過。0.5 前後で数値が読みやすくなる。</summary>
    private static readonly Color VisibleModulate = new(1f, 1f, 1f, 0.5f);

    internal static readonly System.Reflection.FieldInfo? EnergyIconField =
        AccessTools.Field(typeof(NCard), "_unplayableEnergyIcon");

    internal static readonly System.Reflection.FieldInfo? StarIconField =
        AccessTools.Field(typeof(NCard), "_unplayableStarIcon");

    internal static void Apply(NCard card, System.Reflection.FieldInfo? field)
    {
        if (field?.GetValue(card) is not TextureRect icon)
            return;

        icon.Modulate = icon.Visible ? VisibleModulate : Colors.White;
    }
}
