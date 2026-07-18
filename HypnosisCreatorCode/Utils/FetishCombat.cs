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
    public const int FetishDoomFlat = 10;
    public const int FetishDoomMinimum = 11;
    public const decimal BogDoomMultiplier = 1.5M;

    public static FetishType? ToFetishType(OrbModel orb) => orb switch
    {
        SmFetishOrb => FetishType.Sm,
        DsFetishOrb => FetishType.DomSub,
        AbnormalFetishOrb => FetishType.Abnormal,
        TranceFetishOrb => FetishType.Trance,
        _ => null
    };

    public static Type ToOrbType(FetishType type) => type switch
    {
        FetishType.Sm => typeof(SmFetishOrb),
        FetishType.DomSub => typeof(DsFetishOrb),
        FetishType.Abnormal => typeof(AbnormalFetishOrb),
        FetishType.Trance => typeof(TranceFetishOrb),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
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

    /// <summary>同種が無ければスロットを増やして植え付ける。</summary>
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
        return Math.Max(FetishDoomMinimum, fromHp + FetishDoomFlat);
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

    /// <summary>
    /// 性癖刺さり判定。複数ヒットでも1回呼ぶ想定。リプレイ時はプレイごとに呼ぶ。
    /// </summary>
    public static async Task<bool> TryFetishHit(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel card,
        IReadOnlyList<FetishType> cardFetishes,
        bool alwaysHit)
    {
        if (cardFetishes.Count == 0 && !alwaysHit) return false;
        if (!target.IsEnemy) return false;

        var hits = alwaysHit
            ? cardFetishes.DefaultIfEmpty(FetishType.Abnormal).Distinct().ToList()
            : cardFetishes.Where(f => HasFetish(target, f)).Distinct().ToList();

        // 「必ず刺さる」でトリガーだけ欲しい場合も1回は破滅を付与
        if (alwaysHit && hits.Count == 0)
            hits = [FetishType.Abnormal];

        if (hits.Count == 0) return false;

        // 種類ごとに個別判定だが、通常カードはタグ分まとめて1プレイ1回の破滅（壱佰捌煩悩は呼び出し側で分割）
        await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target), applier, card);
        return true;
    }

    /// <summary>壱佰捌煩悩用: 刺さった種類ごとに破滅。</summary>
    public static async Task<int> TryFetishHitPerType(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel card,
        IReadOnlyList<FetishType> cardFetishes,
        bool alwaysHit)
    {
        var types = alwaysHit
            ? cardFetishes.Distinct().ToList()
            : cardFetishes.Where(f => HasFetish(target, f)).Distinct().ToList();

        var count = 0;
        foreach (var _ in types)
        {
            await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target), applier, card);
            count++;
        }

        return count;
    }
}
