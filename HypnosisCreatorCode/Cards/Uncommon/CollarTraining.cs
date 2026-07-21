using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 首輪と調教 — DomSub。対象を「引き寄せ」る。既に引き寄せ済みの相手には破滅を与えDomSub性癖を目覚めさせる。
/// アタックだがダメージは与えない。UGでは破滅発生時にランダムな調教命令カードを2枚手札に加える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CollarTraining() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(PullTracker.IsPulled);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 15M)];

    private static bool IsCommandCard(CardModel c) => c is TrainingCommand;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var alreadyPulled = PullTracker.IsPulled(play.Target);

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
                    var generated = new List<CardModel>(2);
                    for (var i = 0; i < 2; i++)
                    {
                        var canonical = pool[rng.NextInt(pool.Count)];
                        generated.Add(CombatState.CreateCard(canonical, Owner));
                    }

                    await TrainingCommand.AddGeneratedToHandOrderedAsync(generated, Owner);
                }
            }
        }

        // 初回も2回目以降も寄る（移動量は PullTracker 側で半減）
        await PullTracker.TryPull(play.Target, Owner.Creature);

        await PullTracker.TryNunuHellBonusDamageAsync(
            choiceContext, Owner.Creature, play.Target, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() { }
}
