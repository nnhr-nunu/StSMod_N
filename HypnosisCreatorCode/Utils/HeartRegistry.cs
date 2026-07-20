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
        return ByMonsterIdLazy.Value.TryGetValue(monsterIdEntry, out var type) ? type : null;
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
                    // 先勝ち（別名衝突は実装ミス）
                    map.TryAdd(id, type);
                }
            }
            catch
            {
                // 生成できない型はスキップ
            }
        }

        return map;
    }
}
