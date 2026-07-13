using BaseLib.Abstracts;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Character;

public class HypnosisCreatorRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => HypnosisCreator.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}