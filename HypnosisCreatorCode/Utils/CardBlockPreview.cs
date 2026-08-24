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
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true)
    {
        _ = previewMode;
        if (!ShouldUseCombatBlockHooks(card, runGlobalHooks))
            return ApplyEnchantmentModifiers(card, raw, props);

        var owner = card.Owner;
        if (owner?.Creature == null)
            return ApplyEnchantmentModifiers(card, raw, props);

        var combat = card.CombatState ?? owner.Creature.CombatState;
        if (combat == null)
            return ApplyEnchantmentModifiers(card, raw, props);
        if (raw < 0) return 0m;

        try
        {
            var modified = Hook.ModifyBlock(
                combat, owner.Creature, raw, props, card, null, out _);
            modified = Math.Max(modified, 0m);
            return Math.Max(0m, modified);
        }
        catch
        {
            return ApplyEnchantmentModifiers(card, raw, props);
        }
    }

    /// <summary>連続 <c>GainBlock</c> と同じく、各回の修正後ブロックを合算する。</summary>
    public static decimal SumSequentialGains(
        CardModel card,
        int startBlock,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true)
    {
        decimal total = 0m;
        for (var block = startBlock; block >= 0; block--)
            total += ApplyModifiers(card, block, props, previewMode, runGlobalHooks);
        return total;
    }

    public static bool ShouldUseCombatBlockHooks(CardModel card, bool runGlobalHooks)
    {
        if (!runGlobalHooks) return false;
        if (card.IsEnchantmentPreview) return false;
        if (EnchantPreviewUiGuard.IsActive) return false;
        if (card.PreviewOutsideOfCombat) return false;

        var owner = card.Owner;
        if (owner?.Creature == null) return false;

        return (card.CombatState ?? owner.Creature.CombatState) != null;
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

    /// <summary>本家 BlockVar と同様。弱体相当の減少（脆弱）も Max で隠さない。</summary>
    public static void SetBlockPreviewPair(
        CardModel card,
        DynamicVar var,
        decimal rawBase,
        decimal preview,
        ValueProp props)
    {
        var enchantedBase = ApplyEnchantmentModifiers(card, rawBase, props);
        var pair = EnchantPreviewPair.Resolve(
            rawBase, enchantedBase, preview, card.IsEnchantmentPreview);
        var.EnchantedValue = pair.EnchantedValue;
        var.PreviewValue = pair.PreviewValue;
    }

    /// <summary>
    /// 戦闘フックが使えるときは本家 Hook（エンチャント込み）。使えないときはエンチャントのみ。
    /// </summary>
    public static decimal ResolvePreview(
        CardModel card,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true) =>
        ApplyModifiers(card, raw, props, previewMode, runGlobalHooks);
}
