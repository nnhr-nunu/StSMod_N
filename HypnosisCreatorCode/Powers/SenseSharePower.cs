using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 感覚共有 — このターン、単体対象のアタックカードを他の敵にも AutoPlay で波及させる。
/// </summary>
public class SenseSharePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    [ThreadStatic]
    private static bool _resolving;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_resolving) return;
        if (Owner == null || CombatState == null) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var others = CombatState.HittableEnemies.Where(e => e != primary && e.IsAlive).ToList();
        if (others.Count == 0) return;

        _resolving = true;
        try
        {
            var canonical = cardPlay.Card.CanonicalInstance ?? cardPlay.Card;
            foreach (var enemy in others)
            {
                var copy = CombatState.CreateCard(canonical, player);
                await CardCmd.AutoPlay(choiceContext, copy, enemy);
            }
        }
        finally
        {
            _resolving = false;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
