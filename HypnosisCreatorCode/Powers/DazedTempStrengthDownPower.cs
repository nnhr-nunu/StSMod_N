using BaseLib.Abstracts;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>めまい付与の一時的筋力低下（ターン終了で解除）。</summary>
public class DazedTempStrengthDownPower : CustomTemporaryPowerModelWrapper<DazedStatus, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
