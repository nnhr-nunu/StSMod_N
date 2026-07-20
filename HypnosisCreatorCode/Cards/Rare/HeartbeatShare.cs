using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 鼓動の共有 — 保有している心臓レリックの効果を味方と共有する（マルチ専用）。
/// UG: 味方全員へ。共有分は戦闘終了時に消える一時レリック。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartbeatShare() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override bool ShouldGlowWhenConditionMet()
    {
        if (CombatState == null) return false;
        if (HeartInventory.CountHearts(Owner) <= 0) return false;
        return CombatState.Allies.Any(a => a != Owner.Creature && a.IsAlive);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;
        if (HeartInventory.CountHearts(Owner) <= 0) return;

        var allies = CombatState.Allies.Where(a => a != Owner.Creature && a.IsAlive).ToList();
        var targets = IsUpgraded ? allies : allies.Take(1).ToList();

        foreach (var ally in targets)
        {
            var allyPlayer = ally.Player;
            if (allyPlayer == null) continue;
            await HeartbeatShareEffects.ShareAll(choiceContext, Owner, allyPlayer);
        }
    }
}
