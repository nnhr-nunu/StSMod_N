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
/// コピーは正本から作り直す（一時コスト等の状態は持ち越さない）が、アップグレード段階だけは元のカードに合わせる。
/// </summary>
internal static class PropagatedCardPlay
{
    public static async Task OnEnemy(
        PlayerChoiceContext choiceContext,
        ICombatState combatState,
        CardModel source,
        Player player,
        Creature target)
    {
        var copy = combatState.CreateCard(source.CanonicalInstance ?? source, player);
        // CreateCard は常に未アップグレード。UG済みカードの波及が未UG効果になるのを防ぐ。
        for (var level = copy.CurrentUpgradeLevel; level < source.CurrentUpgradeLevel && copy.IsUpgradable; level++)
        {
            copy.UpgradeInternal();
            copy.FinalizeUpgradeInternal();
        }

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
