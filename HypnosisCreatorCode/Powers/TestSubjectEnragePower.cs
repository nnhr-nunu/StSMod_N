using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 激怒（実験体）— 相手がブロックを獲得するたび、筋力 Amount を得る。
/// </summary>
public class TestSubjectEnragePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "test_subject_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "test_subject_heart.png".BigRelicImagePath();

    public override async Task AfterBlockGained(
        Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (amount <= 0 || Amount <= 0) return;
        if (creature == Owner) return;
        if (!creature.IsEnemy) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null!);
    }
}