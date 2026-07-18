using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
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
/// 不動明王 — パワー。自身のデバフを解除し、アーティファクトを得る。
/// CSV: 「デバフを付与した相手」の個別追跡は未実装のため、暫定で敵全体へダメージを与えて近似する。
/// TODO: デバフ付与元を追跡する仕組みが確定したら差し替える。
/// </summary>
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

        if (CombatState != null)
            foreach (var enemy in CombatState.HittableEnemies.ToList())
                await CreatureCmd.Damage(
                    choiceContext, enemy, DynamicVars["Damage"].BaseValue, ValueProp.Move, self, this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ArtifactPower"].UpgradeValueBy(107M);
        DynamicVars["Damage"].UpgradeValueBy(5M);
    }
}
