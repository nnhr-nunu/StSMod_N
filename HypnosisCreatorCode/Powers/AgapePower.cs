using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// Agape — ターン開始時に <see cref="Amount"/> と同量のブロックを得る（残りターン数は別管理）。
/// </summary>
public class AgapePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 調和（No.12）イラストを流用
    public override string CustomPackedIconPath => "harmony.png".CardImagePath();
    public override string CustomBigIconPath => "harmony.png".BigCardImagePath();

    private int _turnsRemaining;

    /// <summary>ターン開始時ブロックを得る残り回数。</summary>
    [SavedProperty]
    public int TurnsRemaining
    {
        get => _turnsRemaining;
        set
        {
            AssertMutable();
            _turnsRemaining = value;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner) return;
        if (TurnsRemaining <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        TurnsRemaining--;
        var block = Math.Max(0, Amount);
        if (block > 0)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, block, ValueProp.Unpowered, null);
        }

        if (TurnsRemaining <= 0)
            await PowerCmd.Remove(this);
    }
}
