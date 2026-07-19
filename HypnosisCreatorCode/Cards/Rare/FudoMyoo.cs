using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 不動明王 — デバフ解除＋アーティファクト2（UG108）。
/// 相手がデバフを付与してくるたびダメージ10（UG15）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FudoMyoo() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    private const decimal ArtifactBase = 2M;
    private const decimal ArtifactUpgraded = 108M;
    private const decimal ReflectBase = 10M;
    private const decimal ReflectUpgradeBonus = 5M;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ArtifactPower>(ArtifactBase),
        new PowerVar<FudoMyooPower>(ReflectBase)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.FromPower<FudoMyooPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;

        foreach (var power in self.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
            await PowerCmd.Remove(power);

        var artifact = IsUpgraded ? ArtifactUpgraded : ArtifactBase;
        DynamicVars["ArtifactPower"].BaseValue = artifact;
        await PowerCmd.Apply<ArtifactPower>(choiceContext, self, artifact, self, this);

        await PowerCmd.Apply<FudoMyooPower>(
            choiceContext, self, DynamicVars["FudoMyooPower"].BaseValue, self, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ArtifactPower"].BaseValue = ArtifactUpgraded;
        DynamicVars["FudoMyooPower"].UpgradeValueBy(ReflectUpgradeBonus);
    }
}
