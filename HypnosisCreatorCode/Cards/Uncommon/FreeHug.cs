using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// フリーハグ — マルチ用。対象を「引き寄せ」る。既に引き寄せ済みの相手には破滅と沼を与え、
/// カードを2枚ランダムな味方に渡す。アタックだがダメージは与えない。
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
        new DynamicVar("Doom", 10M),
        new PowerVar<BogPower>(1M),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var alreadyPulled = PullTracker.IsPulled(play.Target);

        if (alreadyPulled)
        {
            await FetishCombat.ApplyDoom(choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
            await PowerCmd.Apply<BogPower>(
                choiceContext, play.Target, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);

            if (CombatState != null)
            {
                var allies = CombatState.Allies.Where(a => a != Owner.Creature && a.IsAlive).ToList();
                var recipient = allies.Count > 0
                    ? allies[Owner.RunState.Rng.CombatCardSelection.NextInt(allies.Count)].Player
                    : null;

                if (recipient != null)
                {
                    var hand = Owner.PlayerCombatState?.Hand;
                    var toGive = hand?.Cards.Where(c => c != this)
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(DynamicVars.Cards.IntValue)
                        .ToList() ?? [];

                    foreach (var card in toGive)
                        await CardPileCmd.GiveToAnotherPlayer(card, recipient, PileType.Hand);
                }
            }
        }
        else
        {
            await PullTracker.TryPull(play.Target, Owner.Creature);
        }

        await PullTracker.TryNunuHellBonusDamageAsync(
            choiceContext, Owner.Creature, play.Target, this);
    }

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(5M);
}
