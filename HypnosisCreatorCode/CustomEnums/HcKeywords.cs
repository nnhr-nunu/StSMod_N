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

    /// <summary>敵デバフのトランス。説明の [gold]トランス[/gold] とタイトルを一致させる。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword TranceState;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Doom;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Bog;
}

public static class HcUnplayableReasons
{
    [CustomEnum]
    public static UnplayableReason CountNotZero;
}
