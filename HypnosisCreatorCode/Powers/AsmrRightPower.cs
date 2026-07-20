using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>ASMR催眠 — 最後にプレイした側が「右」であることの表示用。</summary>
public class AsmrRightPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "asmr_hypnosis_power.png".PowerImagePath();
    public override string CustomBigIconPath => "asmr_hypnosis_power.png".BigPowerImagePath();
}
