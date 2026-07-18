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

/// <summary>不動明王 — デバフ解除＋アーティファクト。付与してきた敵にダメージ。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FudoMyoo() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ArtifactPower>(1M),
        new DynamicVar("Damage", 5M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArtifactPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;

        foreach (var power in self.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
            await PowerCmd.Remove(power);

        await PowerCmd.Apply<ArtifactPower>(
            choiceContext, self, DynamicVars["ArtifactPower"].BaseValue, self, this);

        var appliers = DebuffSourceTracker.GetAppliers(self)
            .Where(e => e.IsAlive)
            .Distinct()
            .ToList();

        // 記録が空ならフォールバックで全敵（戦闘開始直後など）
        if (appliers.Count == 0 && CombatState != null)
            appliers = CombatState.HittableEnemies.ToList();

        foreach (var enemy in appliers)
            await CreatureCmd.Damage(
                choiceContext, enemy, DynamicVars["Damage"].BaseValue, ValueProp.Move, self, this, play);

        DebuffSourceTracker.Clear(self);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ArtifactPower"].UpgradeValueBy(107M);
        DynamicVars["Damage"].UpgradeValueBy(5M);
    }
}
