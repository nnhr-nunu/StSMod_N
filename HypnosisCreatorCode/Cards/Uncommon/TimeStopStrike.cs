using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 時止めストライク — SM・トランス。トランス中の敵にのみ有効。ダメージはターン終了時にまとめて発生する。
/// 1ターンに3回まで。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TimeStopStrike() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    private const int MaxPlaysPerTurn = 3;
    private readonly PerTurnCounter _plays = new();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm, FetishType.Trance];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!TranceCombat.HasTrance(play.Target)) return;

        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        if (_plays.Increment(turn) > MaxPlaysPerTurn) return;

        await PowerCmd.Apply<TimeStopMarkPower>(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}
