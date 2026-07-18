using BaseLib.Abstracts;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>ぜーろっ用。一時的に筋力を下げて攻撃を0にする。</summary>
public class ZeroOutPower : TemporaryStrengthPower, ICustomModel
{
    public override AbstractModel OriginModel => ModelDb.Card<ZeroOut>();

    protected override bool IsPositive => false;
}
