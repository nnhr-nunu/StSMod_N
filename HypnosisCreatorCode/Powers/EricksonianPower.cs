using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>エリクソン的誘導 — 自分が性癖を刺したとき、手札カウントを1進める（自己バフ）。</summary>
public class EricksonianPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public static Task TryAdvanceHandCountOnFetishHit(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier)
    {
        var player = applier.Player;
        if (player == null) return Task.CompletedTask;
        if (applier.GetPower<EricksonianPower>() == null) return Task.CompletedTask;

        CountRules.AdvanceHandCountCards(player);
        return Task.CompletedTask;
    }
}
