using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// ティンシャ — すべての相手に7ダメージ（UGで9）。1ターンに3回までプレイ可（先2回は手札へ）。
/// 各相手にトランス1。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Tingsha() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    private const int HandReturnsPerTurn = 2;

    private int _playsThisTurn;
    private int _playsTrackedTurn = -1;
    private bool _countedThisPlayWrapper;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7M, ValueProp.Move),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        // 敵ごとに AttackCommand を分けると活力（1回の攻撃ぶんだけ消費）が最初の敵にしか乗らない。全体攻撃は1コマンドで。
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        }
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        // 複製（本家 HistoryCourse 等の CreateDupe）は本家どおり消える。上書き先へ送ると恒久的に1枚増える。
        if (IsDupe) return base.GetResultLocationForCardPlay();
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
