using System.Text;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

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
        try
        {
            if (Activator.CreateInstance(heartType) is EnemyHeartRelic sample)
                return sample.MonsterIdEntries;
        }
        catch
        {
            // ignore
        }

        return [];
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
        foreach (var type in AllHeartTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is not EnemyHeartRelic sample) continue;
                foreach (var id in sample.MonsterIdEntries)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    map.TryAdd(NormalizeMonsterId(id), type);
                }
            }
            catch (Exception e)
            {
                MainFile.Logger.Warn($"HeartRegistry skip {type.Name}: {e.Message}");
            }
        }

        // 旧仮キー → 実ID（セーブ互換・取りこぼし防止）
        TryAlias(map, "SWARMING_HIVE", "SKULKING_COLONY");
        TryAlias(map, "KAISER_CRAB", "CRUSHER");

        MainFile.Logger.Info($"HeartRegistry loaded {map.Count} monster→heart keys");
        return map;
    }

    private static void TryAlias(Dictionary<string, Type> map, string alias, string canonicalId)
    {
        if (!map.TryGetValue(canonicalId, out var type)) return;
        map.TryAdd(alias, type);
    }
}
