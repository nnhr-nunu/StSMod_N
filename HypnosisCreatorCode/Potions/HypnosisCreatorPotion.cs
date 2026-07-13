using BaseLib.Abstracts;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;

namespace HypnosisCreator.HypnosisCreatorCode.Potions;

[Pool(typeof(HypnosisCreatorPotionPool))]
public abstract class HypnosisCreatorPotion : CustomPotionModel;