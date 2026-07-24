using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 状態異常催眠で敵へプレイする呪いの共通基盤。
/// 性癖タグ付きカードは効果後に <see cref="HypnosisCreatorCard.ResolveFetishOnTarget"/> を自動で呼ぶ。
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

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayCurseEffect(choiceContext, play);
        await ResolveFetishAfterEnemyCurseEffect(choiceContext, play);
    }

    protected abstract Task PlayCurseEffect(PlayerChoiceContext choiceContext, CardPlay play);

    private async Task ResolveFetishAfterEnemyCurseEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CardFetishes.Count == 0) return;
        if (play.Target is not { IsEnemy: true, IsAlive: true }) return;
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
