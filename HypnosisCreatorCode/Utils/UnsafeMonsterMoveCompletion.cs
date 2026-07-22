using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Crusher / Rocket 向け PerformMove 差し替え後の本家後処理。
/// <see cref="MonsterModel.PerformMove"/> は OnMovePerformed を呼ばないと
/// ステートマシンが初回ムーブ完了扱いせず、以降の RollMove が空振りして進行不能になる。
/// </summary>
public static class UnsafeMonsterMoveCompletion
{
    /// <summary>
    /// 差し替え行動のあと、本家と同様に「ムーブ実行済み」を立てる。
    /// <paramref name="rollNext"/> が true なら次ターン用に RollMove する（1ターン置換向け）。
    /// </summary>
    public static void AfterSubstitutedPerform(MonsterModel monster, bool rollNext)
    {
        try
        {
            var move = monster.NextMove;
            if (move != null)
                monster.MoveStateMachine?.OnMovePerformed(move);
        }
        catch
        {
            // ステートマシン差異時は続行
        }

        if (!rollNext) return;

        try
        {
            var creature = monster.Creature;
            if (creature is not { IsAlive: true }) return;
            var targets = creature.CombatState?.GetOpponentsOf(creature) ?? [];
            monster.RollMove(targets);
        }
        catch
        {
            // ロール失敗時はバニラ側の次ターン処理に委ねる
        }
    }

    /// <summary>差し替えターンで意図表示を隠す（待ちなし）。</summary>
    public static void HideIntentImmediate(NCreature node)
    {
        try
        {
            node.AnimHideIntent(0);
        }
        catch
        {
            // Intent UI 差異時は無視
        }
    }
}
