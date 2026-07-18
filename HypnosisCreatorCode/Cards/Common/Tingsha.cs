using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>ティンシャ — すべての相手に6ダメージ。手札に戻る。各相手にトランス1。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Tingsha() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6M, ValueProp.Move),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        }
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        new(Owner, PileType.Hand, CardPilePosition.Top);

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}
