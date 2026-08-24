namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 本家 DamageVar / BlockVar の :diff() 用。
/// 付与確認では素値とエンチャント後を並べ、戦闘中はエンチャント後を基準に実プレビュー（弱体で下がってもそのまま）を出す。
/// </summary>
public static class EnchantPreviewPair
{
    public static (decimal EnchantedValue, decimal PreviewValue) Resolve(
        decimal rawBase,
        decimal enchantedBase,
        decimal preview,
        bool isEnchantmentPreview)
    {
        if (isEnchantmentPreview)
            return (rawBase, enchantedBase);

        return (enchantedBase, preview);
    }
}
