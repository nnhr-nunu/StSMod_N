using HypnosisCreator.HypnosisCreatorCode.Utils;
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
/// スライム催眠 — 1ターンだけ意図を粘液付与へ上書きし、見た目・名前をスライム系からランダム差し替え。
/// </summary>
public class SlimeHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public string? DisguiseName { get; private set; }
    private SlimeDisguise.State? _disguise;

    public override Task AfterApplied(Creature applier, CardModel cardSource)
    {
        TryOverwriteIntent();
        TryApplyDisguise(applier);
        return Task.CompletedTask;
    }

    private void TryApplyDisguise(Creature applier)
    {
        if (Owner == null) return;

        var rng = applier.Player?.RunState.Rng.CombatCardSelection
                  ?? Owner.Monster?.CombatState?.Players.FirstOrDefault()?.RunState.Rng.CombatCardSelection;
        if (rng == null) return;

        _disguise = SlimeDisguise.Apply(Owner, rng);
        DisguiseName = _disguise?.DisplayName;
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

        SlimeDisguise.Restore(Owner, _disguise);
        _disguise = null;
        DisguiseName = null;
        await PowerCmd.Remove(this);
    }
}
