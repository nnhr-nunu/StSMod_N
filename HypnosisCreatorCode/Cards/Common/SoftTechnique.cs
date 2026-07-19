using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>軟酥の法 — 自身のデバフを1つ（ランダム）除去する。UGですべて除去。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SoftTechnique() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool ShouldGlowWhenConditionMet() =>
        Owner.Creature.Powers.Any(p => p.Type == PowerType.Debuff);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var debuffs = Owner.Creature.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .ToList();

        if (debuffs.Count == 0) return;

        if (IsUpgraded)
        {
            foreach (var power in debuffs)
                await PowerCmd.Remove(power);
            return;
        }

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var chosen = debuffs[rng.NextInt(debuffs.Count)];
        await PowerCmd.Remove(chosen);
    }
}
