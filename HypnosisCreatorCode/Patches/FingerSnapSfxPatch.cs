using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
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
        if (HcAttackHitSfxRules.UsesVanillaHeavyHit(__instance.ModelSource as CardModel))
            TmpHitSfxField?.SetValue(__instance, VanillaAttackSfx.HeavyHitFile);
        else
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
        typeof(IEnumerable<Creature>),
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
        CardModel? cardSource) =>
        FingerSnapAttackHitSfxLogic.TryPlay(dealer, cardSource);
}

/// <summary>単体ターゲット直呼び用。AttackCommand 本体は IEnumerable 版のみだが保険。</summary>
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
public static class FingerSnapAttackHitSfxSingleTargetPatch
{
    public static void Prefix(
        Creature dealer,
        CardModel? cardSource) =>
        FingerSnapAttackHitSfxLogic.TryPlay(dealer, cardSource);
}

file static class FingerSnapAttackHitSfxLogic
{
    public static void TryPlay(Creature dealer, CardModel? cardSource)
    {
        if (cardSource?.Type != CardType.Attack) return;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(dealer.Player)) return;

        if (cardSource is Tingsha)
        {
            TingshaSfx.Play();
            return;
        }

        if (HcAttackHitSfxRules.UsesSlapHit(cardSource))
        {
            SlapSfx.Play();
            return;
        }

        if (HcAttackHitSfxRules.UsesVanillaHeavyHit(cardSource))
            return;

        if (FingerSnapSfxTracker.TryAdvance(out var totalHits, out var hitIndex))
            FingerSnapSfx.PlayForHit(totalHits, hitIndex);
        else
            FingerSnapSfx.PlayNormal();
    }
}
