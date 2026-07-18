using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>解剖 — 14＋心臓数×4（UG×7）。リーサルで心臓。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Autopsy() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14M, ValueProp.Move),
        new DynamicVar("PerHeart", 4M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hearts = HeartInventory.CountHearts(Owner);
        var damage = DynamicVars.Damage.BaseValue + hearts * DynamicVars["PerHeart"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        if (play.Target is { IsAlive: false })
            await HeartCapture.TryCapture(Owner, play.Target);

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["PerHeart"].UpgradeValueBy(3M); // 4 → 7
}
