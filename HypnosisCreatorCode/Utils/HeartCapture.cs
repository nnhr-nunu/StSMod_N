using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵固有心臓の入手。報酬系（リーサル／寄生／心停止＋）はすべて
/// 戦闘報酬画面の追加 <see cref="RelicReward"/> に載せる（本家の追加報酬 UX）。
/// ムカデ節など「同一心臓を複数体が共有」する場合、リーサル系は最後の1体トドメ時のみ。
/// 心停止催眠＋は例外で、止めた対象からその場で落とす（<c>allowWhileSiblingsAlive</c>）。
/// </summary>
public static class HeartCapture
{
    private static readonly List<(Player Player, string MonsterId)> Pending = [];

    /// <summary>
    /// 戦闘終了時に解決する捕獲を予約する。
    /// 付与対象が死亡してパワーが消えても心臓を落とせるようにする。
    /// </summary>
    public static void QueueCapture(Player player, string monsterIdEntry, Creature? source = null)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return;
        if (!ShouldGrantHeart(source, monsterIdEntry))
        {
            MainFile.Logger.Info(
                $"Heart capture queue skipped (minion without mapped heart): {monsterIdEntry}");
            return;
        }

        Pending.Add((player, monsterIdEntry));
        MainFile.Logger.Info($"Heart capture queued for {monsterIdEntry}");
    }

    public static void QueueCapture(Player player, Creature target)
    {
        if (!target.IsMonster) return;
        var monsterId = HeartRegistry.GetMonsterId(target);
        if (string.IsNullOrWhiteSpace(monsterId)) return;
        QueueCapture(player, monsterId, target);
    }

    /// <summary>予約済み捕獲をすべて追加レリック報酬として解決する（戦闘終了フックから呼ぶ）。</summary>
    public static void FlushPending()
    {
        if (Pending.Count == 0) return;

        var batch = Pending.ToList();
        Pending.Clear();

        // 同心臓タイプは1回だけ（寄生を複数節に付けた場合など）
        var seenHeartTypes = new HashSet<Type>();
        foreach (var (player, monsterId) in batch)
        {
            var heartType = HeartRegistry.ResolveHeartType(monsterId) ?? typeof(StolenHeart);
            if (!seenHeartTypes.Add(heartType))
            {
                MainFile.Logger.Info($"Heart capture skipped (duplicate type {heartType.Name}) for {monsterId}");
                continue;
            }

            TryAddExtraRelicReward(player, monsterId);
        }
    }

    /// <summary>
    /// 戦闘報酬画面に「追加のレリック報酬」として敵固有心臓を載せる。
    /// エリート等の通常報酬と並ぶ。部屋が取れない場合のみ即時 Obtain にフォールバック。
    /// </summary>
    /// <param name="allowWhileSiblingsAlive">
    /// true のとき、同心臓の生存兄弟（ムカデ節など）がいても落とす。
    /// 心停止催眠＋の例外用。解剖／えぐりは false のまま。
    /// </param>
    public static void TryAddExtraRelicReward(
        Player player, Creature slain, bool allowWhileSiblingsAlive = false)
    {
        if (!slain.IsMonster) return;

        if (!allowWhileSiblingsAlive && HasLivingSiblingForSameHeart(slain))
        {
            var mid = HeartRegistry.GetMonsterId(slain) ?? slain.ModelId.Entry;
            MainFile.Logger.Info($"Heart deferred until last sibling dies: {mid}");
            return;
        }

        var monsterId = HeartRegistry.GetMonsterId(slain);
        if (string.IsNullOrWhiteSpace(monsterId))
        {
            if (!ShouldGrantHeart(slain, null))
            {
                MainFile.Logger.Info(
                    $"Heart capture skipped (minion without monster id): ModelId={slain.ModelId.Entry}");
                return;
            }

            MainFile.Logger.Warn(
                $"HeartCapture: no monster id for slain (ModelId={slain.ModelId.Entry}); StolenHeart fallback");
            TryAddExtraRelicReward(player, "", slain);
            return;
        }

        TryAddExtraRelicReward(player, monsterId, slain);
    }

    public static void TryAddExtraRelicReward(Player player, string monsterIdEntry, Creature? source = null)
    {
        if (!ShouldGrantHeart(source, monsterIdEntry))
        {
            MainFile.Logger.Info(
                $"Heart capture skipped (minion without mapped heart): '{monsterIdEntry}'");
            return;
        }

        var relic = CreateHeartRelic(monsterIdEntry);
        if (relic == null)
        {
            MainFile.Logger.Warn($"Failed to create heart relic for '{monsterIdEntry}'");
            return;
        }

        if (relic is StolenHeart)
            MainFile.Logger.Warn(
                $"HeartCapture fell back to StolenHeart for '{monsterIdEntry}' (unmapped monster id)");

        if (player.RunState.CurrentRoom is CombatRoom room)
        {
            room.AddExtraReward(player, new RelicReward(relic, player));
            MainFile.Logger.Info($"Extra relic reward added: {relic.Id.Entry} from '{monsterIdEntry}'");
            return;
        }

        MainFile.Logger.Info($"No CombatRoom for extra reward; fallback Obtain for '{monsterIdEntry}'");
        _ = ObtainNow(player, monsterIdEntry, source);
    }

    /// <summary>
    /// 心臓を落とすか。登録済み ID は常に可。
    /// 未登録は通常敵のみ <see cref="StolenHeart"/> フォールバック可。ミニオンはドロップなし。
    /// </summary>
    internal static bool ShouldGrantHeart(Creature? source, string? monsterIdEntry)
    {
        if (!string.IsNullOrWhiteSpace(monsterIdEntry)
            && HeartRegistry.ResolveHeartType(monsterIdEntry) != null)
            return true;

        return source is not { IsSecondaryEnemy: true };
    }

    /// <summary>
    /// 倒した敵と同じ心臓に紐づく生存敵がまだいるか（ムカデ FRONT/MIDDLE/BACK など）。
    /// いるあいだは心臓を落とさない。
    /// </summary>
    public static bool HasLivingSiblingForSameHeart(Creature slain)
    {
        if (!slain.IsMonster) return false;

        var monsterId = HeartRegistry.GetMonsterId(slain);
        var heartType = monsterId == null ? null : HeartRegistry.ResolveHeartType(monsterId);
        if (heartType == null) return false;

        var combat = slain.CombatState;
        if (combat == null) return false;

        foreach (var enemy in combat.HittableEnemies)
        {
            if (ReferenceEquals(enemy, slain)) continue;
            if (!enemy.IsAlive || !enemy.IsMonster) continue;

            var otherId = HeartRegistry.GetMonsterId(enemy);
            if (otherId != null && HeartRegistry.ResolveHeartType(otherId) == heartType)
                return true;
        }

        return false;
    }

    /// <summary>部屋が無いときなどの即時付与フォールバック。未登録は StolenHeart。</summary>
    public static async Task ObtainNow(Player player, string monsterIdEntry, Creature? source = null)
    {
        if (!ShouldGrantHeart(source, monsterIdEntry))
        {
            MainFile.Logger.Info(
                $"Heart ObtainNow skipped (minion without mapped heart): '{monsterIdEntry}'");
            return;
        }

        MainFile.Logger.Info($"Heart ObtainNow from '{monsterIdEntry}'");

        var relic = CreateHeartRelic(monsterIdEntry);
        if (relic == null)
        {
            await RelicCmd.Obtain<StolenHeart>(player);
            return;
        }

        await RelicCmd.Obtain(relic, player);
    }

    private static RelicModel? CreateHeartRelic(string monsterIdEntry)
    {
        var heartType = string.IsNullOrWhiteSpace(monsterIdEntry)
            ? typeof(StolenHeart)
            : HeartRegistry.ResolveHeartType(monsterIdEntry) ?? typeof(StolenHeart);

        var canonical = ModelDb.AllRelics.FirstOrDefault(r => r.GetType() == heartType);
        if (canonical != null)
            return canonical.ToMutable();

        if (Activator.CreateInstance(heartType) is RelicModel created)
            return created;

        return null;
    }
}
