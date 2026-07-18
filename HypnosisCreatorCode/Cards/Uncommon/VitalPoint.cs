using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>急所の一刺し — アブノーマル。直前にアタックカードをプレイしていないターン数が多いほどダメージが増加する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class VitalPoint() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10M, ValueProp.Move),
        new DynamicVar("PerTurn", 10M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        var gap = PlayerAttackTracker.TurnsSinceLastAttack(Owner, turn);
        var damage = DynamicVars.Damage.BaseValue + gap * DynamicVars["PerTurn"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["PerTurn"].UpgradeValueBy(5M);
}
