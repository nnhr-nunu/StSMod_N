using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>ヒプノクリエイター攻撃ヒット SE のカード別ルール。</summary>
internal static class HcAttackHitSfxRules
{
    /// <summary>腹部への殴打など。本家 heavy_attack.mp3（AttackCommand 側で再生）。</summary>
    public static bool UsesVanillaHeavyHit(CardModel? card) => card is AbdominalStrike;

    /// <summary>スパンキングなど。mod の Hit-Slap.mp3。</summary>
    public static bool UsesSlapHit(CardModel? card) => card is Spanking;
}
