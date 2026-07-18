using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 鼓動の共有 — パワー・ハート・マルチ用。所持する心臓レリックを味方1人と共有する（UGで味方全員）。
/// ソロプレイでは他の味方がいないため実質発動しない。
/// TODO: マルチプレイ環境での動作確認が必要（mechanics-lock.md「マルチ用」参照）。
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
