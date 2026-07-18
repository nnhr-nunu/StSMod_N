using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>軟酥の法 — 自身のデバフをすべて除去する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SoftTechnique() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var debuffs = Owner.Creature.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .ToList();

        foreach (var power in debuffs)
            await PowerCmd.Remove(power);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
