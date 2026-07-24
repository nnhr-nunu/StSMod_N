using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 状態異常催眠／心臓で敵にプレイする状態異常の共通基盤。
/// 性癖タグ付きカードは効果後に <see cref="HypnosisCreatorCard.ResolveFetishOnTarget"/> を自動で呼ぶ。
/// </summary>
public abstract class PlayableStatusCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = false) :
    HypnosisCreatorCard(cost, type, rarity, target, showInCardLibrary)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    /// <summary>
    /// true なら状態異常催眠なしでも敵へプレイ可（心臓付与）。
    /// false ならトランス敵＋状態異常催眠が必要。
    /// </summary>
    public bool FreeEnemyPlay { get; set; }

    protected override bool ShouldGlowWhenConditionMet() =>
        StatusHypnosisRules.CanStartPlay(this);

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayStatusEffect(choiceContext, play);
        await ResolveFetishAfterEnemyStatusEffect(choiceContext, play);
    }

    protected abstract Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play);

    private async Task ResolveFetishAfterEnemyStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CardFetishes.Count == 0) return;
        if (play.Target is not { IsEnemy: true, IsAlive: true }) return;
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
