using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>エリクソン的誘導 — 対象の性癖を刺したとき、付与者の手札カウントを1進める。</summary>
public class EricksonianPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public static Task TryAdvanceHandCountOnFetishHit(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier)
    {
        if (target.GetPower<EricksonianPower>() == null) return Task.CompletedTask;
        var player = applier.Player;
        if (player == null) return Task.CompletedTask;
        CountRules.AdvanceHandCountCards(player);
        return Task.CompletedTask;
    }
}
