using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Relics.Starter;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

internal static class FetishOwnerLookup
{
    public static Player? Find(ICombatState combatState)
    {
        foreach (var player in combatState.Players)
        {
            if (player.Character is HcCharacter) return player;
            if (player.Relics.Any(r => r is FetishBog)) return player;
            if (player.Character?.Id.Entry.Contains("HYPNOSISCREATOR", StringComparison.OrdinalIgnoreCase) == true)
                return player;
        }

        return null;
    }

    public static async Task EnsureAllEnemiesAsync(
        ICombatState combatState,
        PlayerChoiceContext choiceContext)
    {
        var owner = Find(combatState);
        if (owner == null) return;

        var rng = owner.RunState.Rng.CombatOrbGeneration;
        foreach (var enemy in combatState.HittableEnemies.ToList())
            await EnemyFetishSlots.EnsureSpawnDefaultsAsync(choiceContext, enemy, owner, rng);
    }
}

/// <summary>
/// 敵の出現処理（本家が固有パワーを付与する <see cref="Creature.AfterAddedToRoom"/>）の直後に性癖を決めてバフ行へ付与する。
/// 戦闘開始時の準備中は IsInProgress が false なので表示待ちが入らず、途中出現（召喚）でも出現コマンドの await に乗る。
/// 投げっぱなし（fire-and-forget）にすると、付与後フックの完了時刻が端末ごとの早送り設定で変わり共闘で同期がずれる。
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.AfterAddedToRoom))]
public static class FetishSpawnOnAddedToRoomPatch
{
    public static void Postfix(Creature __instance, ref Task __result)
    {
        if (!__instance.IsEnemy || __instance.CombatState is not { } combatState) return;

        __result = ContinueIfHittable(__result, __instance, combatState);
    }

    // 以前の開始時付与（HittableEnemies 対象）と同じく、殴れない敵は殴れるようになったターン開始時まで待つ。
    // 殴れるかは本家の固有パワー（AfterAddedToRoom 本体）付与後に決まるので、await のあとで判定する。
    private static async Task ContinueIfHittable(Task original, Creature enemy, ICombatState combatState)
    {
        await original;
        if (!enemy.IsHittable) return;

        var owner = FetishOwnerLookup.Find(combatState);
        if (owner == null) return;

        var rng = owner.RunState.Rng.CombatOrbGeneration;
        await EnemyFetishSlots.EnsureSpawnDefaultsAsync(new ThrowingPlayerChoiceContext(), enemy, owner, rng);
    }
}

/// <summary>敵出現時の保険（通常は <see cref="FetishSpawnOnAddedToRoomPatch"/> で付与済みなので何もしない）。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCreatureAddedToCombat))]
public static class FetishHudCombatPatch
{
    public static void Postfix(ICombatState combatState, Creature creature, ref Task __result)
    {
        if (!creature.IsEnemy) return;

        var owner = FetishOwnerLookup.Find(combatState);
        if (owner == null) return;

        __result = ContinueWithSpawnDefaults(__result, creature, owner);
    }

    internal static async Task ContinueWithSpawnDefaults(Task original, Creature enemy, Player owner)
    {
        await original;
        var rng = owner.RunState.Rng.CombatOrbGeneration;
        await EnemyFetishSlots.EnsureSpawnDefaultsAsync(new ThrowingPlayerChoiceContext(), enemy, owner, rng);
    }
}

/// <summary>戦闘開始時に全敵を再初期化／再描画。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class FetishHudBeforeCombatPatch
{
    /// <summary>
    /// 戦闘間の static 状態の掃除はフック開始前に行う。戻り値なしの Postfix はフックの最初の待ちの位置で走り、
    /// その位置は早送り設定で変わるため、共闘で他のリスナーとの前後がずれる。
    /// </summary>
    public static void Prefix(ICombatState combatState)
    {
        // Hook.AfterCombatEnd は勝利時にしか呼ばれない。敗北・中断で残った戦闘間の static 状態を開始時にも掃除する。
        FetishCombat.ClearPlayScopes();
        EnemyPlayerAttackTracker.Reset();
        AsmrSideRules.Reset();
        PendingDiscardPileCardPreview.Clear();
        PendingStatusHypnosisConvert.Clear();
        HeartCapture.ClearPending();

        foreach (var player in combatState.Players)
        {
            PlayerAttackTracker.Reset(player);
            HypnosisIntroDrawTracker.Reset(player);
            CognitiveShuffleCompletion.ResetForNewCombat(player);
        }
    }

    public static void Postfix(ICombatState combatState, ref Task __result)
    {
        if (FetishOwnerLookup.Find(combatState) == null) return;

        // GetResult で同期待ちするとフリーズする（CustomScaledWait がゲームループを塞ぐ）。
        // 投げっぱなしも、フックの途中に割り込む位置が早送り設定で変わり、パワーの並び順が端末ごとにずれる
        // （同期チェックは並び順込み）。本家が await するフックの Task に後続として繋ぎ、全リスナーの後に付与する。
        __result = ContinueAsync(__result, combatState);
    }

    private static async Task ContinueAsync(Task original, ICombatState combatState)
    {
        await original;

        var choiceContext = new ThrowingPlayerChoiceContext();
        // お仕置き「対象がプレイヤーを攻撃した回数」: 共闘では誰への攻撃も数えるため全員に付ける（非表示なので待ちなし）
        foreach (var player in combatState.Players)
        {
            if (player.Creature is not { } creature) continue;
            if (creature.GetPower<EnemyPlayerAttackTrackerPower>() != null) continue;
            await PowerCmd.Apply<EnemyPlayerAttackTrackerPower>(
                choiceContext, creature, 1M, creature, null, silent: true);
        }

        await FetishOwnerLookup.EnsureAllEnemiesAsync(combatState, choiceContext);
    }
}

/// <summary>
/// セーブ続き／取りこぼし対策。プレイヤーターン開始時にも全敵へ性癖バフを保証する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class FetishHudTurnStartPatch
{
    public static void Postfix(
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        Player player,
        ref Task __result)
    {
        _ = player;
        if (FetishOwnerLookup.Find(combatState) == null) return;

        var original = __result;
        __result = ContinueAsync(original, combatState, choiceContext);
    }

    private static async Task ContinueAsync(
        Task original,
        ICombatState combatState,
        PlayerChoiceContext choiceContext)
    {
        await original;
        await FetishOwnerLookup.EnsureAllEnemiesAsync(combatState, choiceContext);
    }
}
