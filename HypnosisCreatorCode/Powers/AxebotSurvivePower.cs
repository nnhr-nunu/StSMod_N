using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// アックスマシン — HP0になりそうなとき HP1 で踏みとどまる。
/// Amount＝残り回数。重ねがけで +2。
/// </summary>
public class AxebotSurvivePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "axebot_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "axebot_heart.png".BigRelicImagePath();

    public override bool ShouldDieLate(Creature creature)
    {
        if (Owner == null || creature != Owner) return true;
        return Amount <= 0;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (Owner == null || creature != Owner) return;
        if (Amount <= 0) return;

        await CreatureCmd.SetCurrentHp(creature, 1);
        await PowerCmd.Decrement(this);
    }
}
