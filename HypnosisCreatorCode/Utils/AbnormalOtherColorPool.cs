using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// CSV備考の他色アブノーマル対象カード（他キャラ／無色）。
/// 該当タイプには常にアブノーマル性癖を付与する。
/// </summary>
public static class AbnormalOtherColorPool
{
    private static readonly HashSet<Type> Types =
    [
        typeof(Dismantle),      // 解体
        typeof(Bludgeon),       // 脳天割り
        typeof(Bloodletting),   // 瀉血
        typeof(Feed),           // 捕食
        typeof(Mangle),         // 切り刻み
        typeof(TearAsunder),    // 八つ裂き
        typeof(Brand),          // 焼印
        typeof(DeadlyPoison),   // 致死毒
        typeof(Skewer),         // 串刺し
        typeof(Strangle),       // 絞殺
        typeof(NoxiousFumes),   // 有毒ガス
        typeof(Murder),         // 殺害
        typeof(Adrenaline),     // アドレナリン
        typeof(Bury),           // 埋葬
        typeof(DeathsDoor),     // 死への扉
        typeof(CaptureSpirit),  // 霊魂抽出
        typeof(Lethality),      // 殺意
        typeof(Hang),           // 絞首
        typeof(Reanimate),      // 蘇生
        typeof(GoForTheEyes),   // 目潰し
        typeof(Sunder),         // 切断
        typeof(Compact),        // 圧縮
        typeof(HelixDrill),     // 螺旋ドリル
        typeof(Shatter),        // 粉砕
        typeof(DarkShackles),   // 闇の足枷
        typeof(Rend),           // 裂傷
        typeof(Squash),         // 踏み潰し
        typeof(Shiv),           // ナイフ
    ];

    public static bool Contains(CardModel card) =>
        Types.Contains(card.GetType());

    public static bool Contains(Type type) => Types.Contains(type);

    /// <summary>変換先プール用の正規カード一覧。</summary>
    public static IReadOnlyList<CardModel> GetCanonicalCards() =>
        Types.Select(t => ModelDb.AllCards.First(c => c.GetType() == t)).ToList();
}
