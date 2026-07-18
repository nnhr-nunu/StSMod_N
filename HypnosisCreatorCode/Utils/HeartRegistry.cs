using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// モンスター Id.Entry → 敵固有心臓レリック型。Contains 一致（大文字小文字無視）。
/// 型一覧はアセンブリ内の具象 <see cref="EnemyHeartRelic"/> を実行時収集する（未実装型の参照でビルドを壊さない）。
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

        foreach (var type in AllHeartTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is not EnemyHeartRelic sample) continue;
                var key = sample.MonsterIdEntry;
                if (!string.IsNullOrEmpty(key) &&
                    monsterIdEntry.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return type;
            }
            catch
            {
                // 生成できない型はスキップ
            }
        }

        return null;
    }
}
