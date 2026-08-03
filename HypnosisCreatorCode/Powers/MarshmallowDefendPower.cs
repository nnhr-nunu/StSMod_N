using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// マシュマロ回答 — 次の敵行動をブロック獲得へ差し替える（Crusher / Rocket 等の不安全敵向け）。
/// </summary>
public class MarshmallowDefendPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    private bool _delivered;

    public bool ShouldReplacePerform => !_delivered;

    public async Task TryReplacePerformAsync()
    {
        if (_delivered || Owner is not { IsAlive: true }) return;
        _delivered = true;

        var block = Math.Max(0M, Amount);
        if (block > 0M)
            await CreatureCmd.GainBlock(Owner, block, ValueProp.Move, null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
