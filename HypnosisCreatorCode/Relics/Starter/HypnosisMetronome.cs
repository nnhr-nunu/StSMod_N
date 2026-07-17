using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HypnosisCreator.HypnosisCreatorCode.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>ぬぬはらのメトロノーム。戦闘1ターン目開始時に Dom を得る（スターターからは外し、報酬プール用）。</summary>
public class HypnosisMetronome : HypnosisCreatorRelic
{
    private const int DomAmount = 1;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DominationPower>(DomAmount)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await PowerCmd.Apply<DominationPower>(
            choiceContext,
            Owner.Creature,
            DomAmount,
            Owner.Creature,
            null);
    }
}
