using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 状態異常催眠で敵へプレイする呪いの共通基盤。
/// </summary>
public abstract class PlayableCurseCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = false) :
    HypnosisCreatorCard(cost, type, rarity, target, showInCardLibrary)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    /// <summary>true なら催眠なしでも敵へプレイ可（心臓付与など）。</summary>
    public bool FreeEnemyPlay { get; set; }

    protected override bool ShouldGlowWhenConditionMet() =>
        StatusHypnosisRules.CanStartPlay(this);
}
