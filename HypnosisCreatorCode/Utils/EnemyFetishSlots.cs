using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode.Orbs;
using HypnosisCreator.HypnosisCreatorCode.Orbs.Fetishes;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>敵ごとの性癖スロット。出現時は SM / DomSub / アブノーマルから1つ。</summary>
public static class EnemyFetishSlots
{
    public const int DefaultCapacity = 1;

    // SpireField だと保持に失敗する事例があるため、敵インスタンスに直結する
    private static readonly ConditionalWeakTable<Creature, FetishSlotState> Table = new();

    public static FetishSlotState Get(Creature creature) =>
        Table.GetValue(creature, static _ => new FetishSlotState());

    /// <summary>出現時: スロット1・トランス以外のランダム性癖1つ。</summary>
    public static void EnsureSpawnDefaults(Creature enemy, Player owner, Rng rng)
    {
        if (!enemy.IsEnemy) return;

        try
        {
            var state = Get(enemy);
            if (!state.Initialized)
            {
                state.Capacity = Math.Max(state.Capacity, DefaultCapacity);
                if (state.Fetishes.Count == 0)
                    state.Fetishes.Add(CreateSpawnFetish(owner, rng));

                state.Initialized = true;
                MainFile.Logger.Info(
                    $"Fetish spawn: {enemy.Name} -> {state.Fetishes[0].Id.Entry} (cap={state.Capacity})");
            }

            // オーブHUDは廃止。バフ行の性癖パワーを同期する。
            FetishOrbHud.QueueRefresh(enemy, visible: false);
            FetishCombat.SyncFetishPowers(enemy, owner);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Fetish EnsureSpawnDefaults failed: {e}");
        }
    }

    /// <summary>性癖スロット数を増やす（目覚め時など）。</summary>
    public static int AddCapacity(Creature enemy, int amount)
    {
        if (!enemy.IsEnemy || amount <= 0) return Get(enemy).Capacity;
        var state = Get(enemy);
        state.Capacity += amount;
        FetishOrbHud.QueueRefresh(enemy, visible: false);
        return state.Capacity;
    }

    /// <summary>空スロットへ性癖を植え付ける。同種は追加しない（mechanics-lock）。</summary>
    public static bool TryPlant(Creature enemy, OrbModel fetish, Player owner)
    {
        if (!enemy.IsEnemy) return false;
        if (fetish is not HypnosisCreatorOrb) return false;

        var state = Get(enemy);
        var plantType = FetishCombat.ToFetishType(fetish);
        if (plantType != null && state.Fetishes.Any(o => FetishCombat.ToFetishType(o) == plantType))
            return false;

        if (state.Fetishes.Count >= state.Capacity)
            state.Capacity = state.Fetishes.Count + 1;

        var mutable = fetish.IsMutable ? fetish : fetish.ToMutable(0);
        mutable.Owner = owner;
        state.Fetishes.Add(mutable);
        FetishOrbHud.QueueRefresh(enemy, visible: false);
        FetishCombat.SyncFetishPowers(enemy, owner);
        return true;
    }

    public static bool TryPlant<TFetish>(Creature enemy, Player owner)
        where TFetish : HypnosisCreatorOrb =>
        TryPlant(enemy, ModelDb.Orb<TFetish>(), owner);

    public static bool TryPlantRandom(Creature enemy, Player owner, Rng rng) =>
        TryPlant(enemy, CreateSpawnFetish(owner, rng), owner);

    /// <summary>出現時／ランダム植え付け: SM・DomSub・アブノーマルのみ（トランス除外）。</summary>
    public static OrbModel CreateSpawnFetish(Player owner, Rng rng)
    {
        OrbModel canonical = rng.NextInt(3) switch
        {
            0 => ModelDb.Orb<SmFetishOrb>(),
            1 => ModelDb.Orb<DsFetishOrb>(),
            _ => ModelDb.Orb<AbnormalFetishOrb>()
        };

        var mutable = canonical.ToMutable(0);
        mutable.Owner = owner;
        return mutable;
    }

    [Obsolete("Use CreateSpawnFetish")]
    public static OrbModel CreateRandom(Player owner, Rng rng) => CreateSpawnFetish(owner, rng);
}

public sealed class FetishSlotState
{
    public int Capacity { get; set; } = EnemyFetishSlots.DefaultCapacity;
    public List<OrbModel> Fetishes { get; } = [];
    public bool Initialized { get; set; }
}
