using System.Reflection;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Present! — ランダムなバフ1つを奪う調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Present() : TrainingCommand
{
    private static readonly MethodInfo GenericApply = typeof(PowerCmd)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(m => m.Name == nameof(PowerCmd.Apply) && m.IsGenericMethodDefinition && m.GetParameters().Length == 5);

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => c.Powers.Any(p => p.Type == PowerType.Buff));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var buffs = play.Target.Powers.Where(p => p.Type == PowerType.Buff).ToList();
        if (buffs.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var stolen = buffs[rng.NextInt(buffs.Count)];
        var amount = stolen.Amount;
        var powerType = stolen.GetType();

        await PowerCmd.Remove(stolen);

        try
        {
            var apply = GenericApply.MakeGenericMethod(powerType);
            var task = (Task)apply.Invoke(null, [choiceContext, Owner.Creature, amount, Owner.Creature, this])!;
            await task;
        }
        catch
        {
            // 未知のバフ型は敵から除去のみ（プレイヤーへの付与は best-effort）
        }
    }
}
