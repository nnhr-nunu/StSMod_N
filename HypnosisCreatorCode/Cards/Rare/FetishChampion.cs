using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>性癖の覇者 — 対象の性癖数×20ダメージ（UG25）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishChampion() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Abnormal, FetishType.Sm, FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(20M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hits = FetishCombat.GetFetishes(play.Target).Count;
        if (hits <= 0) return;
        var total = DynamicVars.Damage.BaseValue * hits;

        await DamageCmd.Attack(total)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5M);
}
