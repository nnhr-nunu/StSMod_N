using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ヒプノクリエイターの攻撃ヒット時に指パッチン SE を鳴らし、vanilla の剣／鈍撃音は抑止する。
/// 本家は AttackContext.AddHit ではなく Execute 内の CreatureCmd.Damage を連打するため、Damage 側で鳴らす。
/// </summary>
[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
public static class FingerSnapSuppressVanillaHitSfxPatch
{
    private static readonly FieldInfo? HitSfxField =
        AccessTools.Field(typeof(AttackCommand), "<HitSfx>k__BackingField");

    private static readonly FieldInfo? TmpHitSfxField =
        AccessTools.Field(typeof(AttackCommand), "<TmpHitSfx>k__BackingField");

    private static readonly FieldInfo? HitCountField =
        AccessTools.Field(typeof(AttackCommand), "_hitCount");

    public static void Prefix(AttackCommand __instance)
    {
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(__instance.Attacker?.Player)) return;

        HitSfxField?.SetValue(__instance, "");
        TmpHitSfxField?.SetValue(__instance, "");
        FingerSnapSfxTracker.BeginAttack(ReadHitCount(__instance));
    }

    [HarmonyPriority(Priority.Last)]
    public static void Postfix(AttackCommand __instance, ref Task<AttackCommand> __result)
    {
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(__instance.Attacker?.Player)) return;

        var original = __result;
        __result = EndAttackAfterAsync(original);
    }

    private static async Task<AttackCommand> EndAttackAfterAsync(Task<AttackCommand> original)
    {
        try
        {
            return await original;
        }
        finally
        {
            FingerSnapSfxTracker.EndAttack();
        }
    }

    private static int ReadHitCount(AttackCommand cmd) =>
        HitCountField?.GetValue(cmd) switch
        {
            int i => i,
            _ => 1
        };
}

[HarmonyPatch(
    typeof(CreatureCmd),
    nameof(CreatureCmd.Damage),
    [
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature),
        typeof(CardModel),
        typeof(CardPlay)
    ])]
public static class FingerSnapAttackHitSfxPatch
{
    public static void Prefix(
        Creature dealer,
        CardModel? cardSource)
    {
        if (cardSource?.Type != CardType.Attack) return;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(dealer.Player)) return;

        if (FingerSnapSfxTracker.TryAdvance(out var totalHits, out var hitIndex))
            FingerSnapSfx.PlayForHit(totalHits, hitIndex);
        else
            FingerSnapSfx.PlayNormal();
    }
}
