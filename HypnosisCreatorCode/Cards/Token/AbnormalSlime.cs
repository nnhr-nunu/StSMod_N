using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 粘液 — 0コスト状態異常。アブノーマル目覚め＋1ドロー。廃棄。
/// 状態異常催眠（トランス敵）またはスライムバーサーカー心臓（自由プレイ）で使用可能。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalSlime() : HypnosisCreatorCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.AnyEnemy,
    showInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    /// <summary>生成カードのためスキルポーション等の戦闘中生成対象外。</summary>
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    /// <summary>
    /// true なら状態異常催眠なしでも敵へプレイ可（スライムバーサーカー心臓）。
    /// </summary>
    public bool FreeEnemyPlay { get; set; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    /// <summary>自由プレイ／状態異常催眠＋トランス敵があるとき黄ハイライト。</summary>
    protected override bool ShouldGlowWhenConditionMet() =>
        StatusHypnosisRules.CanStartPlay(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        FetishCombat.Awaken(play.Target, FetishType.Abnormal, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
}
