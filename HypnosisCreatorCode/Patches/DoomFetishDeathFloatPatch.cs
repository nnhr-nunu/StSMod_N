using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 性癖を1つ以上持つ敵が破滅で倒れる直前に、刺さり時と同じ吹き出しを出す。
/// <see cref="DoomPower.DoomKill"/> の Prefix なら Kill / 消滅演出前なのでノードが残っている。
/// </summary>
[HarmonyPatch(typeof(DoomPower), nameof(DoomPower.DoomKill))]
public static class DoomFetishDeathFloatPatch
{
    public static void Prefix(IReadOnlyList<Creature> creatures)
    {
        if (creatures == null || creatures.Count == 0) return;

        foreach (var creature in creatures)
        {
            if (creature is not { IsEnemy: true, IsAlive: true }) continue;
            if (FetishCombat.GetFetishes(creature).Count <= 0) continue;
            FetishHitFloat.Show(creature);
        }
    }
}
