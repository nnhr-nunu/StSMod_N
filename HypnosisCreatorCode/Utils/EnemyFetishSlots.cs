using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Orbs;
using HypnosisCreator.HypnosisCreatorCode.Orbs.Fetishes;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>敵ごとの性癖スロット（オーブ流用）。カードからスロット増加・植え付け可能。</summary>
public static class EnemyFetishSlots
{
    public const int DefaultCapacity = 1;

    private static readonly NotNullSpireField<Creature, FetishSlotState> Field =
        new(() => new FetishSlotState());

    public static FetishSlotState Get(Creature creature) => Field.Get(creature);

    /// <summary>出現時: スロット1・ランダム性癖1つを保証する。</summary>
    public static void EnsureSpawnDefaults(Creature enemy, Player owner, Rng rng)
    {
        if (!enemy.IsEnemy) return;

        var state = Get(enemy);
        if (state.Initialized)
        {
            FetishOrbHud.QueueRefresh(enemy, visible: true);
            return;
        }

        state.Capacity = Math.Max(state.Capacity, DefaultCapacity);
        if (state.Fetishes.Count == 0)
            state.Fetishes.Add(CreateRandom(owner, rng));

        state.Initialized = true;
        FetishOrbHud.QueueRefresh(enemy, visible: true);
    }

    /// <summary>性癖スロット数を増やす（目覚め時など）。頭上HUDも更新。</summary>
    public static int AddCapacity(Creature enemy, int amount)
    {
        if (!enemy.IsEnemy || amount <= 0) return Get(enemy).Capacity;
        var state = Get(enemy);
        state.Capacity += amount;
        FetishOrbHud.QueueRefresh(enemy, visible: true);
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

        // 満杯ならスロットを1増やしてから植える（目覚めの「増えたらスロットも増える」）
        if (state.Fetishes.Count >= state.Capacity)
            state.Capacity = state.Fetishes.Count + 1;

        var mutable = fetish.IsMutable ? fetish : fetish.ToMutable(0);
        mutable.Owner = owner;
        state.Fetishes.Add(mutable);
        FetishOrbHud.QueueRefresh(enemy, visible: true);
        return true;
    }

    public static bool TryPlant<TFetish>(Creature enemy, Player owner)
        where TFetish : HypnosisCreatorOrb =>
        TryPlant(enemy, ModelDb.Orb<TFetish>(), owner);

    public static bool TryPlantRandom(Creature enemy, Player owner, Rng rng) =>
        TryPlant(enemy, CreateRandom(owner, rng), owner);

    public static OrbModel CreateRandom(Player owner, Rng rng)
    {
        OrbModel canonical = rng.NextInt(4) switch
        {
            0 => ModelDb.Orb<SmFetishOrb>(),
            1 => ModelDb.Orb<DsFetishOrb>(),
            2 => ModelDb.Orb<TranceFetishOrb>(),
            _ => ModelDb.Orb<AbnormalFetishOrb>()
        };

        var mutable = canonical.ToMutable(0);
        mutable.Owner = owner;
        return mutable;
    }
}

public sealed class FetishSlotState
{
    public int Capacity { get; set; } = EnemyFetishSlots.DefaultCapacity;
    public List<OrbModel> Fetishes { get; } = [];
    public bool Initialized { get; set; }
}
