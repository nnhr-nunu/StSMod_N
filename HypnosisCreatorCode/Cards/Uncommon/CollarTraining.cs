using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 首輪と調教 — DomSub。対象を「引き寄せ」る。既に引き寄せ済みの相手には破滅を与えDomSub性癖を目覚めさせる。
/// UGでは破滅発生時にランダムな調教命令カードを2枚手札に加える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CollarTraining() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9M, ValueProp.Move),
        new DynamicVar("Doom", 15M)
    ];

    private static bool IsCommandCard(CardModel c) => c is TrainingCommand;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var alreadyPulled = PullTracker.IsPulled(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);

        if (alreadyPulled)
        {
            await FetishCombat.ApplyDoom(choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
            FetishCombat.Awaken(play.Target, FetishType.DomSub, Owner);

            if (IsUpgraded && CombatState != null)
            {
                var pool = ModelDb.AllCards.Where(IsCommandCard).ToList();
                if (pool.Count > 0)
                {
                    var rng = Owner.RunState.Rng.CombatCardSelection;
                    for (var i = 0; i < 2; i++)
                    {
                        var canonical = pool[rng.NextInt(pool.Count)];
                        var generated = CombatState.CreateCard(canonical, Owner);
                        await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, Owner);
                    }
                }
            }
        }
        else
        {
            PullTracker.TryPull(play.Target);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() { }
}
