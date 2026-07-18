using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>トランス付与・判定。仕様は mechanics-lock.md。</summary>
public static class TranceCombat
{
    public static int GetTrance(Creature creature) =>
        creature.GetPowerAmount<TrancePower>();

    public static bool HasTrance(Creature creature) => GetTrance(creature) > 0;

    public static bool AnyEnemyHasTrance(IEnumerable<Creature> enemies) =>
        enemies.Any(e => e.IsAlive && HasTrance(e));

    /// <summary>
    /// トランスを1回付与する（スタック amount）。トランス性癖があれば刺さり1回。
    /// 「トランス1を3回」は本メソッドを3回呼ぶ。
    /// </summary>
    public static async Task ApplyTrance(
        PlayerChoiceContext choiceContext,
        Creature target,
        int amount,
        Creature applier,
        CardModel? cardSource = null)
    {
        if (amount <= 0 || !target.IsEnemy) return;
        await PowerCmd.Apply<TrancePower>(choiceContext, target, amount, applier, cardSource!);
        TranceFallTracker.Add(target, amount);
        await FetishCombat.TryTranceFetishHitOnApply(choiceContext, target, applier, cardSource);
        await NotifyTranceAppliedForCultLeader(choiceContext, applier);
    }

    /// <summary>教祖化: トランス付与時、次のターンのエナジー・ドローを得る。</summary>
    private static async Task NotifyTranceAppliedForCultLeader(PlayerChoiceContext choiceContext, Creature applier)
    {
        var cultLeader = applier.GetPower<CultLeaderPower>();
        if (cultLeader == null) return;

        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, applier, 1M, applier, null);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, applier, cultLeader.Amount, applier, null);
    }

    /// <summary>トランス1を times 回付与（連続トランス用）。</summary>
    public static async Task ApplyTranceRepeated(
        PlayerChoiceContext choiceContext,
        Creature target,
        int times,
        Creature applier,
        CardModel? cardSource = null,
        int amountPer = 1)
    {
        for (var i = 0; i < times; i++)
            await ApplyTrance(choiceContext, target, amountPer, applier, cardSource);
    }
}
