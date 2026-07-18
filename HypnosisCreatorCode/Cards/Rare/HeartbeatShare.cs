using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 鼓動の共有 — 心臓レリック効果を味方と共有する想定。
/// TODO(task.md): 心臓パッシブの実効共有APIが無いため、StolenHeart 付与で近似。
/// ソロでは味方不在のため実質不発。マルチ破壊回避のため best-effort のまま維持。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartbeatShare() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var hearts = Owner.Relics.Count(r => r is StolenHeart);
        if (hearts <= 0) return;

        var allies = CombatState.Allies.Where(a => a != Owner.Creature && a.IsAlive).ToList();
        var targets = IsUpgraded ? allies : allies.Take(1).ToList();

        foreach (var ally in targets)
        {
            var allyPlayer = ally.Player;
            if (allyPlayer == null) continue;
            await RelicCmd.Obtain<StolenHeart>(allyPlayer);
        }
    }
}
