using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// フリーハグ — マルチ用。対象を「引き寄せ」る。既に引き寄せ済みの相手には破滅と沼を与え、
/// カードを2枚ランダムな味方（マルチ用。ソロでは自分）に渡す。
/// TODO: マルチプレイ環境での動作確認が必要（mechanics-lock.md「マルチ用」参照）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FreeHug() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6M, ValueProp.Move),
        new DynamicVar("Doom", 10M),
        new PowerVar<BogPower>(1M),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

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
            PullTracker.TryPull(play.Target);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(5M);
}
