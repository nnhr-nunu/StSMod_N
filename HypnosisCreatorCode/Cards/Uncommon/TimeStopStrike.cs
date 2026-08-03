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
    /// <summary>手札に戻る回数（この回数までプレイ後に手札へ）。</summary>
    private const int HandReturnsPerTurn = 2;

    /// <summary>同一カード実体のターン内プレイ回数（手札戻しで実体が維持される）。</summary>
    private int _playsThisTurn;
    private int _playsTrackedTurn = -1;

    /// <summary>OnPlayWrapper 1回あたり GetResultLocation の複数呼び出しで二重加算しない。</summary>
    private bool _countedThisPlayWrapper;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    // トランス対象がいないとプレイ不可 → 性癖一致だけでは光らせない
    protected override bool FetishGlowAllowed => ShouldGlowWhenConditionMet();

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(TranceCombat.HasTrance);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7M, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<TimeStopMarkPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!TranceCombat.HasTrance(play.Target)) return;

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
    /// 本家 OnPlayWrapper は本メソッドを BeforeCardPlayed より先に複数回呼ぶ。
    /// 1プレイ1回だけ加算し、加算後の回数で行き先を決める。
    /// </summary>
    protected override CardLocation GetResultLocationForCardPlay()
    {
        if (!_countedThisPlayWrapper)
        {
            RecordPlayThisTurn();
            _countedThisPlayWrapper = true;
        }

        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        if (GetPlaysThisTurn(turn) <= HandReturnsPerTurn)
            return new CardLocation(Owner, PileType.Hand, CardPilePosition.Top);

        return base.GetResultLocationForCardPlay();
    }

    /// <summary>OnPlayWrapper 終了後にフラグを戻す。</summary>
    internal void FinishPlayWrapper() => _countedThisPlayWrapper = false;

    private void RecordPlayThisTurn()
    {
        var turn = Owner?.PlayerCombatState?.TurnNumber ?? 0;
        if (_playsTrackedTurn != turn)
        {
            _playsTrackedTurn = turn;
            _playsThisTurn = 0;
        }

        _playsThisTurn++;
    }

    private int GetPlaysThisTurn(int turn)
    {
        if (_playsTrackedTurn != turn)
            return 0;
        return _playsThisTurn;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}
