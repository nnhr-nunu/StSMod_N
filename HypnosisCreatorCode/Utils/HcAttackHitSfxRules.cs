using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>ヒプノクリエイター攻撃ヒット SE のカード別ルール。</summary>
internal static class HcAttackHitSfxRules
{
    /// <summary>腹部への殴打・足蹴など。本家 heavy_attack.mp3。</summary>
    public static bool UsesVanillaHeavyHit(CardModel? card) =>
        card is AbdominalStrike or Kick;

    /// <summary>急所の一刺し・心臓えぐり・解剖など。サイレント系 dagger_throw.mp3。</summary>
    public static bool UsesVanillaKnifeHit(CardModel? card) =>
        card is VitalPoint or HeartGouge or Autopsy;

    /// <summary>スパンキング・お仕置き・躾。mod Hit-Slap.mp3。</summary>
    public static bool UsesSlapHit(CardModel? card) =>
        card is Spanking or Punishment or Discipline;
}
