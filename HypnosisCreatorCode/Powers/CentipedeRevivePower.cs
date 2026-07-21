using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 万足ムカデ — 死亡時に HP25 で復活。スタック数＝復活回数。
/// 心臓レリック右クリック／No.86 再使用で重ねがけ可能。
/// </summary>
public class CentipedeRevivePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "centipede_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "centipede_heart.png".BigRelicImagePath();

    public override bool ShouldDieLate(Creature creature)
    {
        if (Owner == null || creature != Owner) return true;
        return Amount <= 0;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (Owner == null || creature != Owner) return;
        if (Amount <= 0) return;

        await CreatureCmd.Heal(creature, 25);
        await PowerCmd.Decrement(this);
    }
}
