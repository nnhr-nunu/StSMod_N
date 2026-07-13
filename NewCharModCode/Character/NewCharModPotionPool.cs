using BaseLib.Abstracts;
using NewCharMod.NewCharModCode.Extensions;
using Godot;

namespace NewCharMod.NewCharModCode.Character;

public class NewCharModPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => NewCharMod.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}