using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 時止めストライク — SM（トランス中の敵にのみプレイ可）。ダメージはターン終了時にまとめて発生する。
/// 1ターンに3回までプレイ可＝先の2回は手札に戻り、3回目で通常どおり捨て札へ。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TimeStopStrike() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    /// <summary>手札に戻る回数（この回数まではプレイ後に手札へ）。</summary>
    private const int HandReturnsPerTurn = 2;

    private readonly PerTurnCounter _plays = new();

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    // トランス対象がいないとプレイ不可 → 性癖一致だけでは光らせない
    protected override bool FetishGlowAllowed => ShouldGlowWhenConditionMet();

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(TranceCombat.HasTrance);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6M, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<TimeStopMarkPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!TranceCombat.HasTrance(play.Target)) return;

        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        _plays.Increment(turn);

        await PowerCmd.Apply<TimeStopMarkPower>(
            choiceContext,
            play.Target,
            CardSourceDamageBonus.AmountToStack(
                this, play.Target, play, DynamicVars.Damage.BaseValue, ValueProp.Move),
            Owner.Creature,
            this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    /// <summary>
    /// このターンのプレイ前カウントが HandReturnsPerTurn 未満なら手札へ戻す。
    /// （1・2回目→手札、3回目→捨て札。結果として最大3回プレイできる）
    /// GetResultLocation は OnPlay より前に呼ばれる想定。
    /// </summary>
    protected override CardLocation GetResultLocationForCardPlay()
    {
        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        if (_plays.Get(turn) < HandReturnsPerTurn)
            return new CardLocation(Owner, PileType.Hand, CardPilePosition.Top);

        return base.GetResultLocationForCardPlay();
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}
