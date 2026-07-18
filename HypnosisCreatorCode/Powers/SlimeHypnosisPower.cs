using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// スライム催眠 — 1ターンだけ意図を粘液付与へ上書き。見た目差し替えは任意スタブ。
/// </summary>
public class SlimeHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature applier, CardModel cardSource)
    {
        TryOverwriteIntent();
        return Task.CompletedTask;
    }

    private void TryOverwriteIntent()
    {
        if (Owner?.Monster == null || CombatState == null) return;
        var count = Math.Max(1, Amount);
        var combat = CombatState;

        try
        {
            async Task OnPerform(IReadOnlyList<Creature> targets)
            {
                foreach (var target in targets)
                {
                    var player = target.Player;
                    if (player == null) continue;
                    for (var i = 0; i < count; i++)
                    {
                        var slime = combat.CreateCard(ModelDb.Card<Slimed>(), player);
                        await CardPileCmd.AddGeneratedCardToCombat(slime, PileType.Discard, player);
                    }
                }
            }

            var move = new MoveState(
                "hypnosis_creator_slime_intent",
                OnPerform,
                [new StatusIntent(count)]);
            Owner.Monster.SetMoveImmediate(move, forceTransition: true);
        }
        catch
        {
            // Intent API 差異時はトランス付与のみで継続
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Enemy) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
