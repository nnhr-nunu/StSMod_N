using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>ASMR催眠 — プレイヤーは「右」担当、敵は最後の攻撃者が「右」担当だったことを示す。</summary>
public class AsmrRightPower : HypnosisCreatorPower
{
    /// <summary>
    /// プレイヤーの担当マーカーとして付与するとき、付与前に立てる。付与の途中は Owner が未設定で弱体扱いになり、
    /// プレイヤーのアーティファクトに防がれて（1消費して）表示が付かなかった。
    /// </summary>
    public bool ForPlayer { get; set; }

    public override PowerType Type =>
        ForPlayer || (IsMutable && Owner is { IsPlayer: true }) ? PowerType.Buff : PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "asmr_hypnosis_power.png".PowerImagePath();
    public override string CustomBigIconPath => "asmr_hypnosis_power.png".BigPowerImagePath();

    public override LocString Description =>
        Owner is { IsPlayer: true }
            ? new LocString(base.Description.LocTable, "HYPNOSISCREATOR-ASMR_RIGHT_POWER.description_player")
            : new LocString(base.Description.LocTable, "HYPNOSISCREATOR-ASMR_RIGHT_POWER.description_enemy");
}
