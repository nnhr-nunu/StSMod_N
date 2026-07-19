using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.CustomEnums;

public static class HcKeywords
{
    /// <summary>カウント軸: 解決後コストが0のときだけプレイ可能。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Count;

    /// <summary>性癖タグ。説明への自動挿入はせず、FetishCardText が合成する。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Sm;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword DomSub;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Abnormal;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Trance;
}

public static class HcUnplayableReasons
{
    [CustomEnum]
    public static UnplayableReason CountNotZero;
}
