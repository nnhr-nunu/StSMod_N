using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 不動明王 — デバフ解除＋アーティファクト1（UG108）。
/// デバフを付与してきた敵に5ダメージ（UG10）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FudoMyoo() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    private const decimal ArtifactBase = 1M;
    private const decimal ArtifactUpgraded = 108M;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ArtifactPower>(ArtifactBase),
        new DamageVar(5M, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArtifactPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;

        foreach (var power in self.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
            await PowerCmd.Remove(power);

        // IsUpgraded を正として付与（表示用 DynamicVar と必ず一致させる）
        var artifact = IsUpgraded ? ArtifactUpgraded : ArtifactBase;
        DynamicVars["ArtifactPower"].BaseValue = artifact;
        await PowerCmd.Apply<ArtifactPower>(choiceContext, self, artifact, self, this);

        var appliers = DebuffSourceTracker.GetAppliers(self)
            .Where(e => e.IsAlive)
            .Distinct()
            .ToList();

        // 記録が空ならフォールバックで全敵（戦闘開始直後など）
        if (appliers.Count == 0 && CombatState != null)
            appliers = CombatState.HittableEnemies.ToList();

        foreach (var enemy in appliers)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);

        DebuffSourceTracker.Clear(self);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ArtifactPower"].BaseValue = ArtifactUpgraded;
        DynamicVars.Damage.UpgradeValueBy(5M);
    }
}
