using System.Threading;
using HypnosisCreator.HypnosisCreatorCode.Orbs.Fetishes;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Relics.Starter;
using MegaCrit.Sts2.Core.Combat;
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
    public const int FetishDoomFlat = 5;
    public const int FetishDoomMinimum = 8;
    public const decimal BogDoomMultiplier = 1.5M;
    /// <summary>性癖の深淵 — 刺さり破滅の追加倍率（沼の1.5倍とは別枠で乗算）。</summary>
    public const decimal FetishAbyssDoomMultiplier = 1.5M;

    /// <summary>ぜんぶ知ってるよ 用。刺さり破滅倍率（既定1）。戦闘終了時にリセットされる。</summary>
    public static decimal FetishHitMultiplier { get; set; } = 1M;

    /// <summary>
    /// 教祖化 用。有効な間、SM・DomSub・アブノーマルの性癖カードは対象の性癖有無に関わらず必ず刺さる
    /// （トランス性癖は対象外）。戦闘終了時にリセットされる。
    /// </summary>
    public static bool CultLeaderActive { get; set; }

    /// <summary>
    /// カードプレイ開始時点の沼スタック。同一プレイ内で付与した沼は×1.5に使わない。
    /// AutoPlay 入れ子に備えスタックする。
    /// </summary>
    private static readonly AsyncLocal<Stack<Dictionary<Creature, int>>?> BogSnapshotStack = new();

    /// <summary>
    /// 同一カードプレイ中に目覚めた性癖。そのプレイの刺さり判定からは除外する（次のカード／リプレイから有効）。
    /// </summary>
    private static readonly AsyncLocal<Stack<HashSet<(Creature Enemy, FetishType Type)>>?> AwakenedThisPlayStack = new();

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
        // TryPlant 内で SyncFetishPowers する
        var planted = type switch
        {
            FetishType.Sm => EnemyFetishSlots.TryPlant<SmFetishOrb>(enemy, owner),
            FetishType.DomSub => EnemyFetishSlots.TryPlant<DsFetishOrb>(enemy, owner),
            FetishType.Abnormal => EnemyFetishSlots.TryPlant<AbnormalFetishOrb>(enemy, owner),
            FetishType.Trance => EnemyFetishSlots.TryPlant<TranceFetishOrb>(enemy, owner),
            _ => false
        };
        if (planted)
            MarkAwakenedThisPlay(enemy, type);
        return planted;
    }

    public static bool WasAwakenedThisPlay(Creature enemy, FetishType type)
    {
        var stack = AwakenedThisPlayStack.Value;
        if (stack is not { Count: > 0 }) return false;
        return stack.Peek().Contains((enemy, type));
    }

    private static void MarkAwakenedThisPlay(Creature enemy, FetishType type)
    {
        var stack = AwakenedThisPlayStack.Value;
        if (stack is not { Count: > 0 }) return;
        stack.Peek().Add((enemy, type));
    }

    public static void PushAwakenPlayScope()
    {
        var stack = AwakenedThisPlayStack.Value;
        if (stack == null)
        {
            stack = new Stack<HashSet<(Creature Enemy, FetishType Type)>>();
            AwakenedThisPlayStack.Value = stack;
        }

        stack.Push([]);
    }

    public static void PopAwakenPlayScope()
    {
        var stack = AwakenedThisPlayStack.Value;
        if (stack == null || stack.Count == 0) return;
        stack.Pop();
        if (stack.Count == 0)
            AwakenedThisPlayStack.Value = null;
    }

    public static void ClearAwakenPlayScopes() => AwakenedThisPlayStack.Value = null;

    /// <summary>
    /// スロット上の性癖をバフ行のパワーとして同期する（表示＋ツールチップ用）。
    /// 同期フックから呼ぶ。GetResult 禁止 — 表示パワーの CustomScaledWait でゲームループが止まりフリーズする。
    /// </summary>
    public static void SyncFetishPowers(Creature enemy, Player owner)
    {
        if (!enemy.IsEnemy || owner.Creature == null) return;
        _ = SyncFetishPowersAsync(enemy, owner);
    }

    private static async Task SyncFetishPowersAsync(Creature enemy, Player owner)
    {
        try
        {
            var amount = CalcFetishDoomAmount(enemy, owner.Creature);
            // 選択は発生しない表示同期。null だと AfterPowerAmountChanged で落ちうる。
            var ctx = new ThrowingPlayerChoiceContext();
            await EnsureFetishPowerAsync<SmFetishPower>(ctx, enemy, owner, FetishType.Sm, amount);
            await EnsureFetishPowerAsync<DsFetishPower>(ctx, enemy, owner, FetishType.DomSub, amount);
            await EnsureFetishPowerAsync<AbnormalFetishPower>(ctx, enemy, owner, FetishType.Abnormal, amount);
            await EnsureFetishPowerAsync<TranceFetishPower>(ctx, enemy, owner, FetishType.Trance, amount);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Fetish SyncFetishPowers failed: {e}");
        }
    }

    private static async Task EnsureFetishPowerAsync<TPower>(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        Player owner,
        FetishType type,
        int amount)
        where TPower : FetishAttributePower
    {
        if (!HasFetish(enemy, type)) return;

        var existing = enemy.GetPower<TPower>();
        if (existing != null)
        {
            // ツールチップの破滅Nをこの敵の実計算値に保つ
            if (existing.Amount != amount)
            {
                await PowerCmd.ModifyAmount(
                    choiceContext, existing, amount - existing.Amount, owner.Creature, null, silent: true);
            }
            return;
        }

        // silent: 戦闘開始時のフラッシュ連打を避ける（CustomScaledWait 自体は表示パワーなら動くが、待たない）
        await PowerCmd.Apply<TPower>(choiceContext, enemy, amount, owner.Creature, null, silent: true);
    }

    /// <summary>敵性癖HUD／表示用。「○が性癖。性癖に刺さる行動を受けた時、破滅Nを得る。」</summary>
    public static string FormatEnemyFetishTooltip(FetishType type, int doomAmount)
    {
        var name = FetishDisplayName(type);
        if (IsJapaneseUi())
            return $"{name}が性癖。性癖に刺さる行動を受けた時、破滅{doomAmount}を得る。";
        return $"{name} fetish. When receiving an action that hits this fetish, gain {doomAmount} Doom.";
    }

    public static string FormatEnemyFetishTooltip(FetishType type, Creature enemy, Creature? applier = null) =>
        FormatEnemyFetishTooltip(type, CalcFetishDoomAmount(enemy, applier));

    public static string FetishDisplayName(FetishType type) => type switch
    {
        FetishType.Sm => "SM",
        FetishType.DomSub => "DomSub",
        FetishType.Abnormal => IsJapaneseUi() ? "アブノーマル" : "Abnormal",
        FetishType.Trance => IsJapaneseUi() ? "トランス" : "Trance",
        _ => type.ToString()
    };

    public static bool OwnerHasFetishAbyss(Creature? applier) =>
        applier?.Player?.Relics.Any(r => r is FetishAbyss) == true;

    private static bool IsJapaneseUi()
    {
        try
        {
            var lang = MegaCrit.Sts2.Core.Localization.LocManager.Instance?.Language ?? "";
            return lang.Contains("jpn", StringComparison.OrdinalIgnoreCase)
                   || lang.Contains("ja", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static void AwakenAll(Creature enemy, Player owner)
    {
        foreach (FetishType type in Enum.GetValues<FetishType>())
            Awaken(enemy, type, owner);
    }

    public static int CalcFetishDoomAmount(Creature enemy, Creature? applier = null)
    {
        var fromHp = (int)Math.Ceiling(enemy.MaxHp * (double)FetishDoomHpPercent);
        var baseAmount = Math.Max(FetishDoomMinimum, fromHp + FetishDoomFlat);
        var amount = Math.Max(1, (int)Math.Floor(baseAmount * (double)FetishHitMultiplier));
        if (OwnerHasFetishAbyss(applier))
            amount = Math.Max(1, (int)Math.Floor(amount * (double)FetishAbyssDoomMultiplier));
        return amount;
    }

    public static void PushBogSnapshot(ICombatState combatState)
    {
        var snap = new Dictionary<Creature, int>();
        foreach (var enemy in combatState.HittableEnemies)
            snap[enemy] = enemy.GetPowerAmount<BogPower>();

        var stack = BogSnapshotStack.Value;
        if (stack == null)
        {
            stack = new Stack<Dictionary<Creature, int>>();
            BogSnapshotStack.Value = stack;
        }

        stack.Push(snap);
    }

    public static void PopBogSnapshot()
    {
        var stack = BogSnapshotStack.Value;
        if (stack == null || stack.Count == 0) return;
        stack.Pop();
        if (stack.Count == 0)
            BogSnapshotStack.Value = null;
    }

    public static void ClearBogSnapshots() => BogSnapshotStack.Value = null;

    public static void ClearPlayScopes()
    {
        ClearBogSnapshots();
        ClearAwakenPlayScopes();
    }

    /// <summary>
    /// 沼×1.5。カードプレイ中はプレイ開始前の沼のみ見る（同一プレイで付与した沼は対象外）。
    /// </summary>
    public static int ScaleDoomByBog(Creature enemy, int amount)
    {
        if (amount <= 0) return 0;
        if (GetBogAmountForDoomScale(enemy) <= 0) return amount;
        return Math.Max(1, (int)Math.Floor(amount * (double)BogDoomMultiplier));
    }

    private static int GetBogAmountForDoomScale(Creature enemy)
    {
        var stack = BogSnapshotStack.Value;
        if (stack is { Count: > 0 })
        {
            var snap = stack.Peek();
            return snap.TryGetValue(enemy, out var bog) ? bog : 0;
        }

        return enemy.GetPowerAmount<BogPower>();
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
        // 同一プレイで目覚めたばかりのトランス性癖は、この付与では刺さらない
        if (WasAwakenedThisPlay(target, FetishType.Trance)) return;
        await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target, applier), applier, cardSource);
        FetishHitFloat.Show(target);
    }

    /// <summary>
    /// カードタグによる刺さり。singleHit=true なら一致があっても破滅は1回（感度3000倍）。
    /// false なら種類ごと（足蹴・壱佰など）。
    /// 同一プレイで目覚めた性癖は除外（必中・教祖化は従来どおり）。
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
                .Where(f =>
                {
                    if (CultLeaderActive && f != FetishType.Trance) return true;
                    if (WasAwakenedThisPlay(target, f)) return false;
                    return HasFetish(target, f);
                })
                .Distinct()
                .ToList();
        }

        if (types.Count == 0) return 0;

        if (singleHit)
        {
            await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target, applier), applier, card);
            FetishHitFloat.Show(target);
            await EricksonianPower.TryAdvanceHandCountOnFetishHit(choiceContext, target, applier);
            return 1;
        }

        var count = 0;
        foreach (var _ in types)
        {
            await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target, applier), applier, card);
            count++;
        }

        if (count > 0)
        {
            FetishHitFloat.Show(target);
            await EricksonianPower.TryAdvanceHandCountOnFetishHit(choiceContext, target, applier);
        }

        return count;
    }
}
