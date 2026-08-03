using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>ASMR催眠 — プレイヤーは「左」担当、敵は最後の攻撃者が「左」担当だったことを示す。</summary>
public class AsmrLeftPower : HypnosisCreatorPower
{
    public override PowerType Type =>
        Owner is { IsPlayer: true } ? PowerType.Buff : PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "asmr_hypnosis_power.png".PowerImagePath();
    public override string CustomBigIconPath => "asmr_hypnosis_power.png".BigPowerImagePath();

    public override LocString Description =>
        Owner is { IsPlayer: true }
            ? new LocString(base.Description.LocTable, "HYPNOSISCREATOR-ASMR_LEFT_POWER.description_player")
            : new LocString(base.Description.LocTable, "HYPNOSISCREATOR-ASMR_LEFT_POWER.description_enemy");
}
