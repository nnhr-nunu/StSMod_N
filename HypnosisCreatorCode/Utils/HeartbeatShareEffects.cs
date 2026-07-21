using HypnosisCreator.HypnosisCreatorCode.Relics;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 鼓動の共有 — 所持心臓の効果を味方へ共有する。
/// 所有者の心臓は消費せず、味方に一時レリックとして付与／発動する。
/// </summary>
public static class HeartbeatShareEffects
{
    public static async Task ShareAll(
        PlayerChoiceContext choiceContext,
        Player owner,
        Player ally)
    {
        if (ally.Creature == null) return;

        // ToList: Obtain 中に Relics が変わる
        foreach (var relic in owner.Relics.ToList())
        {
            switch (relic)
            {
                case StolenHeart:
                    await ShareStolenHeart(choiceContext, ally);
                    break;
                case EnemyHeartRelic heart:
                    await ShareEnemyHeart(choiceContext, ally, heart);
                    break;
            }
        }
    }

    private static async Task ShareStolenHeart(PlayerChoiceContext choiceContext, Player ally)
    {
        var obtained = await RelicCmd.Obtain<StolenHeart>(ally);
        if (obtained is HypnosisCreatorRelic hc)
            hc.RemoveAtCombatEnd = true;

        // StolenHeart は戦闘1ターン目のみブロック。共有が2ターン目以降なら即時付与する。
        var turn = ally.PlayerCombatState?.TurnNumber ?? 0;
        if (turn > 1 && ally.Creature != null)
        {
            await CreatureCmd.GainBlock(
                ally.Creature,
                HeartActivationHelpers.BlockAmountWithDexterity(ally.Creature, 2m),
                ValueProp.Unpowered,
                null);
        }
    }

    private static async Task ShareEnemyHeart(
        PlayerChoiceContext choiceContext,
        Player ally,
        EnemyHeartRelic source)
    {
        var canonical = ModelDb.AllRelics.FirstOrDefault(r => r.GetType() == source.GetType());
        RelicModel? obtained;
        if (canonical != null)
            obtained = await RelicCmd.Obtain(canonical.ToMutable(), ally);
        else if (Activator.CreateInstance(source.GetType()) is RelicModel created)
            obtained = await RelicCmd.Obtain(created, ally);
        else
            return;

        if (obtained is HypnosisCreatorRelic hc)
            hc.RemoveAtCombatEnd = true;

        if (obtained is not EnemyHeartRelic shared) return;

        // 希少は所有者を消費せず、共有コピーだけを発動する。
        if (shared.IsRareHeart && !shared.IsUsedUp)
            await shared.ActivateAsync(choiceContext, ally);
    }
}
