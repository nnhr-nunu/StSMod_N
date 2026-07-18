using HypnosisCreator.HypnosisCreatorCode.Orbs.Fetishes;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>性癖刺さり・目覚め・破滅付与。仕様は mechanics-lock.md。</summary>
public static class FetishCombat
{
    public const decimal FetishDoomHpPercent = 0.05M;
    public const int FetishDoomFlat = 7;
    public const int FetishDoomMinimum = 8;
    public const decimal BogDoomMultiplier = 1.5M;

    /// <summary>ぜんぶ知ってるよ 用。刺さり破滅倍率（既定1）。戦闘終了時にリセットされる。</summary>
    public static decimal FetishHitMultiplier { get; set; } = 1M;

    /// <summary>
    /// 教祖化 用。有効な間、SM・DomSub・アブノーマルの性癖カードは対象の性癖有無に関わらず必ず刺さる
    /// （トランス性癖は対象外）。戦闘終了時にリセットされる。
    /// </summary>
    public static bool CultLeaderActive { get; set; }

    public static FetishType? ToFetishType(OrbModel orb) => orb switch
    {
        SmFetishOrb => FetishType.Sm,
        DsFetishOrb => FetishType.DomSub,
        AbnormalFetishOrb => FetishType.Abnormal,
        TranceFetishOrb => FetishType.Trance,
        _ => null
    };

    public static bool HasFetish(Creature enemy, FetishType type)
    {
        if (!enemy.IsEnemy) return false;
        return EnemyFetishSlots.Get(enemy).Fetishes.Any(o => ToFetishType(o) == type);
    }

    public static IReadOnlyList<FetishType> GetFetishes(Creature enemy)
    {
        if (!enemy.IsEnemy) return [];
        return EnemyFetishSlots.Get(enemy).Fetishes
            .Select(ToFetishType)
            .Where(t => t != null)
            .Select(t => t!.Value)
            .Distinct()
            .ToList();
    }

    public static bool Awaken(Creature enemy, FetishType type, Player owner)
    {
        if (!enemy.IsEnemy) return false;
        if (HasFetish(enemy, type)) return false;

        EnemyFetishSlots.AddCapacity(enemy, 1);
        return type switch
        {
            FetishType.Sm => EnemyFetishSlots.TryPlant<SmFetishOrb>(enemy, owner),
            FetishType.DomSub => EnemyFetishSlots.TryPlant<DsFetishOrb>(enemy, owner),
            FetishType.Abnormal => EnemyFetishSlots.TryPlant<AbnormalFetishOrb>(enemy, owner),
            FetishType.Trance => EnemyFetishSlots.TryPlant<TranceFetishOrb>(enemy, owner),
            _ => false
        };
    }

    public static void AwakenAll(Creature enemy, Player owner)
    {
        foreach (FetishType type in Enum.GetValues<FetishType>())
            Awaken(enemy, type, owner);
    }

    public static int CalcFetishDoomAmount(Creature enemy)
    {
        var fromHp = (int)Math.Ceiling(enemy.MaxHp * (double)FetishDoomHpPercent);
        var baseAmount = Math.Max(FetishDoomMinimum, fromHp + FetishDoomFlat);
        return Math.Max(1, (int)Math.Floor(baseAmount * (double)FetishHitMultiplier));
    }

    public static int ScaleDoomByBog(Creature enemy, int amount)
    {
        if (amount <= 0) return 0;
        if (enemy.GetPowerAmount<BogPower>() <= 0) return amount;
        return Math.Max(1, (int)Math.Floor(amount * (double)BogDoomMultiplier));
    }

    public static async Task ApplyDoom(
        PlayerChoiceContext choiceContext,
        Creature target,
        int amount,
        Creature applier,
        CardModel? cardSource)
    {
        if (amount <= 0) return;
        var scaled = ScaleDoomByBog(target, amount);
        await PowerCmd.Apply<DoomPower>(choiceContext, target, scaled, applier, cardSource!);
    }

    /// <summary>トランス付与1回ごとのトランス性癖刺さり。</summary>
    public static async Task TryTranceFetishHitOnApply(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel? cardSource)
    {
        if (!HasFetish(target, FetishType.Trance)) return;
        await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target), applier, cardSource);
    }

    /// <summary>
    /// カードタグによる刺さり。singleHit=true なら一致があっても破滅は1回（感度3000倍）。
    /// false なら種類ごと（足蹴・壱佰など）。
    /// </summary>
    public static async Task<int> TryFetishHit(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel card,
        IReadOnlyList<FetishType> cardFetishes,
        bool alwaysHit,
        bool singleHit = false)
    {
        if (cardFetishes.Count == 0 && !alwaysHit) return 0;
        if (!target.IsEnemy) return 0;

        List<FetishType> types;
        if (alwaysHit)
        {
            types = cardFetishes.Count > 0
                ? cardFetishes.Distinct().ToList()
                : [FetishType.Abnormal];
        }
        else
        {
            types = cardFetishes
                .Where(f => HasFetish(target, f) || (CultLeaderActive && f != FetishType.Trance))
                .Distinct()
                .ToList();
        }

        if (types.Count == 0) return 0;

        if (singleHit)
        {
            await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target), applier, card);
            await EricksonianPower.TryAdvanceHandCountOnFetishHit(choiceContext, target, applier);
            return 1;
        }

        var count = 0;
        foreach (var _ in types)
        {
            await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target), applier, card);
            count++;
        }

        if (count > 0)
            await EricksonianPower.TryAdvanceHandCountOnFetishHit(choiceContext, target, applier);

        return count;
    }
}
