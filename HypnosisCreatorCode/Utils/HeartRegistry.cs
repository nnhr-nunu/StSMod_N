using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// モンスター Id.Entry → 敵固有心臓レリック型。
/// Contains 一致（大文字小文字無視）。複数候補時は最長キー優先。
/// </summary>
public static class HeartRegistry
{
    private static readonly Lazy<IReadOnlyList<Type>> AllTypesLazy = new(() =>
        typeof(EnemyHeartRelic).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(EnemyHeartRelic)))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList());

    public static IReadOnlyList<Type> AllHeartTypes => AllTypesLazy.Value;

    public static Type? ResolveHeartType(string monsterIdEntry)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return null;

        Type? best = null;
        var bestKeyLen = -1;

        foreach (var type in AllHeartTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is not EnemyHeartRelic sample) continue;
                var key = sample.MonsterIdEntry;
                if (string.IsNullOrEmpty(key)) continue;
                if (!monsterIdEntry.Contains(key, StringComparison.OrdinalIgnoreCase)) continue;

                // 完全一致を最優先。部分一致は最長キー。
                var keyLen = key.Length;
                var exact = monsterIdEntry.Equals(key, StringComparison.OrdinalIgnoreCase);
                if (exact)
                    return type;

                if (keyLen > bestKeyLen)
                {
                    best = type;
                    bestKeyLen = keyLen;
                }
            }
            catch
            {
                // 生成できない型はスキップ
            }
        }

        return best;
    }
}
