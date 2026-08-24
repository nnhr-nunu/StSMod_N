using System.Collections.Concurrent;
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
    public const decimal FetishDoomHpPercent = 0.02M;
    public const int FetishDoomFlat = 7;
    public const decimal BogDoomMultiplier = 1.5M;
    /// <summary>性癖の深淵 — 刺さり破滅の追加倍率（沼の1.5倍とは別枠で乗算）。</summary>
    public const decimal FetishAbyssDoomMultiplier = 1.5M;

    /// <summary>ぜんぶ知ってるよの総倍率（未所持なら1）。パワー Amount＝倍率（2＝2倍）。</summary>
    public static decimal ResolveFetishHitMultiplier(Creature? applier)
    {
        var power = applier?.GetPower<KnowItAllPower>();
        if (power == null || power.Amount <= 0) return 1M;
        return power.Amount;
    }

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

    /// <summary>敵ごとの性癖パワー同期を直列化（マルチで fire-and-forget が競合しないように）。</summary>
    private static readonly ConcurrentDictionary<Creature, SemaphoreSlim> SyncLocks = new();

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

    public static async Task<bool> AwakenAsync(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        FetishType type,
        Player owner)
    {
        if (!enemy.IsEnemy) return false;
        if (HasFetish(enemy, type)) return false;

        EnemyFetishSlots.AddCapacity(enemy, 1);
        var planted = type switch
        {
            FetishType.Sm => await EnemyFetishSlots.TryPlantAsync<SmFetishOrb>(choiceContext, enemy, owner),
            FetishType.DomSub => await EnemyFetishSlots.TryPlantAsync<DsFetishOrb>(choiceContext, enemy, owner),
            FetishType.Abnormal => await EnemyFetishSlots.TryPlantAsync<AbnormalFetishOrb>(choiceContext, enemy, owner),
            FetishType.Trance => await EnemyFetishSlots.TryPlantAsync<TranceFetishOrb>(choiceContext, enemy, owner),
            _ => false
        };
        if (planted)
            MarkAwakenedThisPlay(enemy, type);
        return planted;
    }

    [Obsolete("Use AwakenAsync during card play / any synchronized combat action.")]
    public static bool Awaken(Creature enemy, FetishType type, Player owner)
    {
        if (!enemy.IsEnemy) return false;
        if (HasFetish(enemy, type)) return false;

        EnemyFetishSlots.AddCapacity(enemy, 1);
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
    /// 戦闘フックからの呼び出しは fire-and-forget。カードプレイ中は <see cref="SyncFetishPowersAsync"/> を await する。
    /// </summary>
    public static void SyncFetishPowers(Creature enemy, Player owner)
    {
        if (!enemy.IsEnemy || owner.Creature == null) return;
        _ = SyncFetishPowersAsync(new ThrowingPlayerChoiceContext(), enemy, owner);
    }

    public static Task SyncFetishPowersAsync(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        Player owner)
    {
        if (!enemy.IsEnemy || owner.Creature == null) return Task.CompletedTask;
        return SyncFetishPowersCoreAsync(choiceContext, enemy, owner);
    }

    private static async Task SyncFetishPowersCoreAsync(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        Player owner)
    {
        var gate = SyncLocks.GetOrAdd(enemy, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            // バフ行 Amount は全員共通の基礎値（HP2%＋7）。深淵・ぜんぶ知ってるよ等は刺さり時の applier で反映。
            var amount = CalcFetishDoomDisplayAmount(enemy);
            await EnsureFetishPowerAsync<SmFetishPower>(choiceContext, enemy, owner, FetishType.Sm, amount);
            await EnsureFetishPowerAsync<DsFetishPower>(choiceContext, enemy, owner, FetishType.DomSub, amount);
            await EnsureFetishPowerAsync<AbnormalFetishPower>(choiceContext, enemy, owner, FetishType.Abnormal, amount);
            await EnsureFetishPowerAsync<TranceFetishPower>(choiceContext, enemy, owner, FetishType.Trance, amount);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Fetish SyncFetishPowers failed: {e}");
        }
        finally
        {
            gate.Release();
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
            // バフ行は基礎破滅量。刺さり時の実量は CalcFetishDoomAmount(applier) を参照。
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

    public static async Task AwakenAllAsync(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        Player owner)
    {
        foreach (FetishType type in Enum.GetValues<FetishType>())
            await AwakenAsync(choiceContext, enemy, type, owner);
    }

    public static void AwakenAll(Creature enemy, Player owner)
    {
        foreach (FetishType type in Enum.GetValues<FetishType>())
#pragma warning disable CS0618
            Awaken(enemy, type, owner);
#pragma warning restore CS0618
    }

    /// <summary>
    /// 敵バフ行の性癖パワー表示用。マルチで全員が同じ Amount になるよう applier 倍率は含めない。
    /// 実際の刺さりは <see cref="CalcFetishDoomAmount"/> を使う。
    /// </summary>
    public static int CalcFetishDoomDisplayAmount(Creature enemy) =>
        CalcFetishDoomAmount(enemy, applier: null);

    public static int CalcFetishDoomAmount(Creature enemy, Creature? applier = null)
    {
        var fromHp = (int)Math.Ceiling(enemy.MaxHp * (double)FetishDoomHpPercent);
        var baseAmount = fromHp + FetishDoomFlat;
        var amount = Math.Max(1, (int)Math.Floor(baseAmount * (double)ResolveFetishHitMultiplier(applier)));
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
    /// カードタグによる刺さり。1プレイあたり破滅は1回（複数タグ・必中でも重ねない）。
    /// 同一プレイで目覚めた性癖は必中でも除外（目覚めと同時刺さり禁止）。
    /// </summary>
    public static async Task<int> TryFetishHit(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel card,
        IReadOnlyList<FetishType> cardFetishes,
        bool alwaysHit)
    {
        if (cardFetishes.Count == 0 && !alwaysHit) return 0;
        if (!target.IsEnemy) return 0;

        List<FetishType> types;
        if (alwaysHit)
        {
            types = (cardFetishes.Count > 0
                ? cardFetishes.Distinct()
                : [FetishType.Abnormal])
                .Where(f => !WasAwakenedThisPlay(target, f))
                .ToList();
        }
        else
        {
            types = cardFetishes
                .Where(f =>
                {
                    if (WasAwakenedThisPlay(target, f)) return false;
                    if (CultLeaderActive && f != FetishType.Trance) return true;
                    return HasFetish(target, f);
                })
                .Distinct()
                .ToList();
        }

        if (types.Count == 0) return 0;

        await ApplyDoom(choiceContext, target, CalcFetishDoomAmount(target, applier), applier, card);
        FetishHitFloat.Show(target);
        await EricksonianPower.TryAdvanceHandCountOnFetishHit(choiceContext, target, applier);
        return 1;
    }
}
