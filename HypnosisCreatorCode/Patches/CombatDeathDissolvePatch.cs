using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Spine 無し連番立ち絵の死亡演出。
/// 本家は SetAnimationTrigger("Dead") を Spine ありのときだけ呼ぶため、
/// プレイヤー敗北（StartDeathAnim 未使用）と敵撃破の両方をここで補う。
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.InvokeDiedEvent))]
public static class CombatDeathDissolveOnDiedPatch
{
    public static void Postfix(Creature __instance)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(__instance);
        CombatFrameAnimator.TryBeginDeathDissolve(node);
    }
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class CombatDeathDissolveStartDeathAnimPatch
{
    public static bool Prefix(NCreature __instance, bool shouldRemove, ref float __result)
    {
        // Spine ありキャラは本家の Dead トリガー＋AnimDie に任せる
        if (__instance.HasSpineAnimation) return true;
        if (!CombatFrameAnimator.TryBeginDeathDissolve(__instance)) return true;

        var existing = __instance.DeathAnimationTask;
        if (existing != null && !existing.IsCompleted)
        {
            __result = 0f;
            return false;
        }

        __instance.DisableInteractionForDeath();
        foreach (var intent in __instance.IntentContainer.GetChildren().OfType<NIntent>())
            intent.SetFrozen(isFrozen: true);

        __result = CombatDeathDissolve.DurationSeconds;
        __instance.DeathAnimationTask = RunDissolveDeathAnim(__instance, shouldRemove,
            __instance.DeathAnimCancelToken.Token);
        TaskHelper.RunSafely(__instance.DeathAnimationTask);
        return false;
    }

    private static async Task RunDissolveDeathAnim(
        NCreature creature, bool shouldRemove, CancellationToken cancelToken)
    {
        var disableUiTween = creature.AnimDisableUi();
        if (shouldRemove)
            creature.AnimHideIntent();

        await Cmd.Wait(CombatDeathDissolve.DurationSeconds, cancelToken, ignoreCombatEnd: true);
        if (cancelToken.IsCancellationRequested) return;

        if (!shouldRemove) return;

        if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant
            && disableUiTween.IsValid()
            && disableUiTween.IsRunning()
            && !await disableUiTween.AwaitFinished(creature))
        {
            return;
        }

        creature.QueueFreeSafely();
    }
}
