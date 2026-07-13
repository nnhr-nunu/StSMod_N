using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.CustomEnums;

public static class HcKeywords
{
    /// <summary>カウント軸: 解決後コストが0のときだけプレイ可能。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Count;
}

public static class HcUnplayableReasons
{
    [CustomEnum]
    public static UnplayableReason CountNotZero;
}
