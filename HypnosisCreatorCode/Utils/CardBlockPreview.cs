using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カード説明／枠のブロックプレビュー用。敏捷・エンチャント等を本家 <see cref="Hook.ModifyBlock"/> で反映する。
/// </summary>
public static class CardBlockPreview
{
    public static decimal ApplyModifiers(
        CardModel card,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        _ = previewMode;
        var owner = card.Owner;
        if (owner?.Creature == null) return raw;

        var combat = card.CombatState ?? owner.Creature.CombatState;
        if (combat == null) return raw;
        if (raw < 0) return 0m;

        try
        {
            var modified = Hook.ModifyBlock(
                combat, owner.Creature, raw, props, card, null, out _);
            modified = Math.Max(modified, 0m);
            return CombatPreviewText.RoundDisplayAmount(modified);
        }
        catch
        {
            return raw;
        }
    }

    /// <summary>連続 <c>GainBlock</c> と同じく、各回の修正後ブロックを合算する。</summary>
    public static decimal SumSequentialGains(
        CardModel card,
        int startBlock,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        decimal total = 0m;
        for (var block = startBlock; block >= 0; block--)
        {
            var enchanted = ApplyEnchantmentModifiers(card, block, props);
            total += ApplyModifiers(card, enchanted, props, previewMode);
        }
        return total;
    }

    /// <summary>本家 BlockVar と同様にエンチャントの加算・倍率を反映する。</summary>
    public static decimal ApplyEnchantmentModifiers(
        CardModel card,
        decimal raw,
        ValueProp props)
    {
        var enchant = card.Enchantment;
        if (enchant == null) return raw;

        try
        {
            var additive = enchant.EnchantBlockAdditive(raw);
            var multiplicative = enchant.EnchantBlockMultiplicative(additive);
            return Math.Max(multiplicative, 0m);
        }
        catch
        {
            return raw;
        }
    }

    /// <summary>ブロック用。エンチャント付与プレビューでも :diff() が効くようにする。</summary>
    public static void SetBlockPreviewPair(
        CardModel card,
        DynamicVar var,
        decimal rawBase,
        decimal preview,
        ValueProp props)
    {
        var enchantedBase = ApplyEnchantmentModifiers(card, rawBase, props);

        if (card.IsEnchantmentPreview)
        {
            var.EnchantedValue = rawBase;
            var.PreviewValue = Math.Max(preview, enchantedBase);
            return;
        }

        if (card.Enchantment != null)
        {
            var.EnchantedValue = enchantedBase;
            var.PreviewValue = preview;
            return;
        }

        var.EnchantedValue = rawBase;
        var.PreviewValue = preview;
    }
}
