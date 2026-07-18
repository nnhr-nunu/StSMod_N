using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>ぬぬはらのメトロノーム。戦闘1ターン目開始時に敵全体へトランス1。</summary>
public class HypnosisMetronome : HypnosisCreatorRelic
{
    private const int TranceAmount = 1;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", TranceAmount)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (Owner.PlayerCombatState.TurnNumber > 1) return;
        if (Owner.Creature.CombatState == null) return;

        Flash();
        foreach (var enemy in Owner.Creature.CombatState.HittableEnemies.ToList())
        {
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, TranceAmount, Owner.Creature, cardSource: null);
        }
    }
}
