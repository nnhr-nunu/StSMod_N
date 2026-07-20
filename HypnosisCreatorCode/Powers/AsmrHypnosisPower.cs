using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ASMR催眠 — プレイヤーを Left/Right に割り当て、左右交互のプレイで破滅を付与する。
/// ソロでは自己交互（1プレイおき）で同様に動作する。
/// 最後にプレイされた側は AsmrLeftPower / AsmrRightPower で表示する。
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
            await SyncSideMarker(choiceContext, isLeft);
            return;
        }

        if (_lastWasLeft == isLeft)
        {
            await SyncSideMarker(choiceContext, isLeft);
            return;
        }

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            var rng = player.RunState.Rng.CombatCardSelection;
            var target = enemies[rng.NextInt(enemies.Count)];
            await FetishCombat.ApplyDoom(choiceContext, target, Amount, Owner, cardPlay.Card);
        }

        _lastWasLeft = isLeft;
        await SyncSideMarker(choiceContext, isLeft);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await ClearSideMarkers(oldOwner);
        await base.AfterRemoved(oldOwner);
    }

    private async Task SyncSideMarker(PlayerChoiceContext choiceContext, bool isLeft)
    {
        if (Owner == null) return;

        if (isLeft)
        {
            var right = Owner.GetPower<AsmrRightPower>();
            if (right != null) await PowerCmd.Remove(right);
            if (Owner.GetPower<AsmrLeftPower>() == null)
                await PowerCmd.Apply<AsmrLeftPower>(choiceContext, Owner, 1M, Owner, null!, silent: true);
        }
        else
        {
            var left = Owner.GetPower<AsmrLeftPower>();
            if (left != null) await PowerCmd.Remove(left);
            if (Owner.GetPower<AsmrRightPower>() == null)
                await PowerCmd.Apply<AsmrRightPower>(choiceContext, Owner, 1M, Owner, null!, silent: true);
        }
    }

    private static async Task ClearSideMarkers(Creature owner)
    {
        var left = owner.GetPower<AsmrLeftPower>();
        if (left != null) await PowerCmd.Remove(left);
        var right = owner.GetPower<AsmrRightPower>();
        if (right != null) await PowerCmd.Remove(right);
    }
}
