using System.Reflection;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Present! — ランダムなバフ1つを奪う調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Present() : TrainingCommand
{
    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => c.Powers.Any(BuffStripRules.CanStripFromEnemy));

    protected override async Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var buffs = play.Target.Powers.Where(BuffStripRules.CanStripFromEnemy).ToList();
        if (buffs.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var stolen = buffs[rng.NextInt(buffs.Count)];
        var amount = stolen.Amount;
        var powerType = stolen.GetType();

        await PowerCmd.Remove(stolen);

        try
        {
            await LoveHypnosisRedirect.ApplyBuffToPlayer(
                choiceContext, powerType, amount, Owner.Creature, Owner.Creature, this);
        }
        catch
        {
            // 未知のバフ型は敵から除去のみ（プレイヤーへの付与は best-effort）
        }
    }
}
