using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// フリーハグ — マルチ用。引き寄せ＋手札2枚パス（毎回）。
/// 引き寄せ済みの相手には破滅・沼。戦闘中の引き寄せ回数（他カードの引き寄せ含む）に応じて破滅が増加。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FreeHug() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(PullTracker.IsPulled);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Doom", 7M),
        new DynamicVar("DoomPerPull", 2M),
        new PowerVar<BogPower>(1M),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var pullCountBeforePull = PullTracker.GetPullCount(play.Target);

        if (pullCountBeforePull > 0)
        {
            var doomPerPull = DynamicVars["DoomPerPull"].IntValue;
            var totalDoom = DynamicVars["Doom"].IntValue + pullCountBeforePull * doomPerPull;

            await FetishCombat.ApplyDoom(choiceContext, play.Target, totalDoom, Owner.Creature, this);
            await PowerCmd.Apply<BogPower>(
                choiceContext, play.Target, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
        }

        TryEnqueuePassToRandomAlly();

        await PullTracker.TryPull(play.Target, Owner.Creature, choiceContext, this);

        await PullTracker.TryNunuHellBonusDamageAsync(
            choiceContext, Owner.Creature, play.Target, this);
    }

    protected override void OnUpgrade() => DynamicVars["DoomPerPull"].UpgradeValueBy(2M);

    private void TryEnqueuePassToRandomAlly()
    {
        if (CombatState == null) return;

        var teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c != Owner.Creature && c.IsAlive && c.IsPlayer && c.Player != Owner)
            .ToList();

        if (teammates.Count == 0) return;

        var recipientCreature = Owner.RunState.Rng.CombatTargets.NextItem(teammates);
        var recipient = recipientCreature?.Player;
        if (recipient == null) return;

        var hand = Owner.PlayerCombatState?.Hand;
        var candidates = hand?.Cards.Where(c => c != this).ToList() ?? [];
        var toPass = SelectRandomHandCards(candidates, DynamicVars.Cards.IntValue);

        if (toPass.Count > 0)
            PendingCardPassToPlayer.Enqueue(toPass, recipient, this);
    }

    private List<CardModel> SelectRandomHandCards(List<CardModel> candidates, int count)
    {
        if (candidates.Count == 0 || count <= 0)
            return [];

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var pool = candidates.ToList();
        var selected = new List<CardModel>(Math.Min(count, pool.Count));
        for (var i = 0; i < count && pool.Count > 0; i++)
        {
            var idx = rng.NextInt(pool.Count);
            selected.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return selected;
    }
}
