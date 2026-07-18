using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ASMR催眠 — プレイヤーを Left/Right に割り当て、左右交互のプレイで破滅を付与する。
/// ソロでは自己交互（1プレイおき）で同様に動作する。
/// </summary>
public class AsmrHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool? _lastWasLeft;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || CombatState == null) return;
        var player = cardPlay.Card.Owner;
        if (player == null) return;
        if (!CombatState.Players.Contains(player)) return;

        var players = CombatState.Players.ToList();
        bool isLeft;
        if (players.Count <= 1)
            isLeft = !(_lastWasLeft ?? false);
        else
        {
            var index = players.IndexOf(player);
            isLeft = index <= 0;
        }

        if (_lastWasLeft is null)
        {
            _lastWasLeft = isLeft;
            return;
        }

        if (_lastWasLeft == isLeft) return;

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            var rng = player.RunState.Rng.CombatCardSelection;
            var target = enemies[rng.NextInt(enemies.Count)];
            await FetishCombat.ApplyDoom(choiceContext, target, Amount, Owner, cardPlay.Card);
        }

        _lastWasLeft = isLeft;
    }
}
