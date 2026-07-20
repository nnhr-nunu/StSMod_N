using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 性癖の深淵 — 性癖の沼のエンシェント強化（Touch of Orobas）。
/// 沼スターターの効果に加え、性癖刺さり時の破滅が50%増加する（沼の1.5倍とは別枠）。
/// </summary>
public class FetishAbyss : FetishBog
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>これ以上の Orobas 強化はない。</summary>
    public override RelicModel GetUpgradeReplacement() => null!;
}
