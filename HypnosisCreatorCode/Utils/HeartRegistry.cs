using System.Runtime.CompilerServices;
using System.Text;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// モンスター Id.Entry → 敵固有心臓レリック型。
/// 実ゲーム ID と完全一致（大文字小文字無視）。別名は各心臓の <see cref="EnemyHeartRelic.MonsterIdEntries"/>。
/// </summary>
public static class HeartRegistry
{
    private static readonly Lazy<IReadOnlyList<Type>> AllTypesLazy = new(() =>
        typeof(EnemyHeartRelic).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(EnemyHeartRelic)))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList());

    private static readonly Lazy<IReadOnlyDictionary<string, Type>> ByMonsterIdLazy = new(BuildByMonsterId);

    public static IReadOnlyList<Type> AllHeartTypes => AllTypesLazy.Value;

    public static Type? ResolveHeartType(string monsterIdEntry)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return null;

        var key = NormalizeMonsterId(monsterIdEntry);
        if (ByMonsterIdLazy.Value.TryGetValue(key, out var type))
            return type;

        return null;
    }

    /// <summary>戦闘中敵の除外判定など、心臓が持つ全モンスター ID。</summary>
    public static IReadOnlyList<string> GetMonsterIds(Type heartType)
    {
        var sample = TrySampleHeart(heartType);
        return sample?.MonsterIdEntries ?? [];
    }

    /// <summary>
    /// クリーチャーから心臓解決用のモンスター ID を取り出す。
    /// Monster.Id → ModelId → 型名スネーク、の順。
    /// </summary>
    public static string? GetMonsterId(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        if (creature.Monster != null)
        {
            var entry = creature.Monster.Id.Entry;
            if (!string.IsNullOrWhiteSpace(entry))
                return NormalizeMonsterId(entry);
        }

        var modelEntry = creature.ModelId.Entry;
        if (!string.IsNullOrWhiteSpace(modelEntry))
            return NormalizeMonsterId(modelEntry);

        var typeName = creature.Monster?.GetType().Name;
        if (!string.IsNullOrEmpty(typeName))
            return PascalToSnakeUpper(typeName);

        return null;
    }

    public static string NormalizeMonsterId(string monsterIdEntry)
    {
        var id = monsterIdEntry.Trim();
        // "MONSTER.SKULKING_COLONY" / "MONSTER:SKULKING_COLONY"
        var sep = id.LastIndexOfAny(['.', ':']);
        if (sep >= 0 && sep < id.Length - 1)
            id = id[(sep + 1)..];
        return id;
    }

    private static string PascalToSnakeUpper(string pascal)
    {
        if (string.IsNullOrEmpty(pascal)) return pascal;
        var sb = new StringBuilder(pascal.Length * 2);
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(pascal[i - 1]))
                sb.Append('_');
            sb.Append(char.ToUpperInvariant(c));
        }

        return sb.ToString();
    }

    private static IReadOnlyDictionary<string, Type> BuildByMonsterId()
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        // CustomRelicModel は Activator.CreateInstance 不可（ログ: HeartRegistry loaded 0 keys）。
        // 登録済み ModelDb インスタンスを優先し、無い型だけ未初期化オブジェクトで ID 定数を読む。
        foreach (var relic in ModelDb.AllRelics.OfType<EnemyHeartRelic>())
            RegisterIds(map, relic.GetType(), relic.MonsterIdEntries);

        foreach (var type in AllHeartTypes)
        {
            if (map.Values.Contains(type)) continue;
            var sample = TrySampleUninitialized(type);
            if (sample == null) continue;
            RegisterIds(map, type, sample.MonsterIdEntries);
        }

        // 旧仮キー → 実ID（セーブ互換・取りこぼし防止）
        TryAlias(map, "SWARMING_HIVE", "SKULKING_COLONY");
        TryAlias(map, "KAISER_CRAB", "CRUSHER");

        if (map.Count == 0)
            MainFile.Logger.Error("HeartRegistry loaded 0 monster→heart keys — all hearts will fall back to StolenHeart");
        else
            MainFile.Logger.Info($"HeartRegistry loaded {map.Count} monster→heart keys");

        return map;
    }

    private static void RegisterIds(
        Dictionary<string, Type> map, Type heartType, IReadOnlyList<string>? ids)
    {
        if (ids == null) return;
        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            map.TryAdd(NormalizeMonsterId(id), heartType);
        }
    }

    private static EnemyHeartRelic? TrySampleHeart(Type heartType)
    {
        var fromDb = ModelDb.AllRelics.FirstOrDefault(r => r.GetType() == heartType) as EnemyHeartRelic;
        if (fromDb != null) return fromDb;
        return TrySampleUninitialized(heartType);
    }

    /// <summary>
    /// ctor を走らせずに MonsterIdEntry 定数を読む。
    /// フィールド初期化付きプロパティは null になり得るため、式本体の Entries を使うこと。
    /// </summary>
    private static EnemyHeartRelic? TrySampleUninitialized(Type type)
    {
        try
        {
            return (EnemyHeartRelic)RuntimeHelpers.GetUninitializedObject(type);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn(
                $"HeartRegistry uninitialized sample failed {type.Name}: {e.GetBaseException().Message}");
            return null;
        }
    }

    private static void TryAlias(Dictionary<string, Type> map, string alias, string canonicalId)
    {
        if (!map.TryGetValue(canonicalId, out var type)) return;
        map.TryAdd(alias, type);
    }
}
