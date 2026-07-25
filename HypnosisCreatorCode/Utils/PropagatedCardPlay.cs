using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 単体カードを他敵へ波及させるときの共通処理。
/// CreateCard→AutoPlay のコピーが捨て札・廃棄札に残らないよう戦闘から除去する。
/// </summary>
internal static class PropagatedCardPlay
{
    public static async Task OnEnemy(
        PlayerChoiceContext choiceContext,
        ICombatState combatState,
        CardModel canonical,
        Player player,
        Creature target)
    {
        var copy = combatState.CreateCard(canonical, player);
        try
        {
            await CardCmd.AutoPlay(choiceContext, copy, target);
        }
        finally
        {
            await CardPileCmd.RemoveFromCombat(copy);
        }
    }
}
