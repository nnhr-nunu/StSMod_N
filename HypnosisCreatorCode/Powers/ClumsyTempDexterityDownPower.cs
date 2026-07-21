using BaseLib.Abstracts;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>不器用付与の一時的敏捷低下（ターン終了で解除）。</summary>
public class ClumsyTempDexterityDownPower : CustomTemporaryPowerModelWrapper<ClumsyCurse, DexterityPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
