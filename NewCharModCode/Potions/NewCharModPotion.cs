using BaseLib.Abstracts;
using BaseLib.Utils;
using NewCharMod.NewCharModCode.Character;

namespace NewCharMod.NewCharModCode.Potions;

[Pool(typeof(NewCharModPotionPool))]
public abstract class NewCharModPotion : CustomPotionModel;