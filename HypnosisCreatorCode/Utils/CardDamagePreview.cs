using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カード説明／枠のダメージプレビュー用。筋力・弱体などを本家と同じ Hook.ModifyDamage で反映する。
/// </summary>
public static class CardDamagePreview
{
    public static decimal ApplyModifiers(
        CardModel card,
        Creature? target,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        var owner = card.Owner;
        if (owner?.Creature == null) return raw;

        var combat = card.CombatState ?? owner.Creature.CombatState;
        if (combat == null) return raw;

        try
        {
            return Hook.ModifyDamage(
                owner.RunState,
                combat,
                target,
                owner.Creature,
                raw,
                props,
                card,
                cardPlay: null,
                ModifyDamageHookType.All,
                previewMode,
                out _);
        }
        catch
        {
            return raw;
        }
    }

    /// <summary>
    /// Enchanted＝修正前、Preview＝修正後。:diff() の緑表示に使う。
    /// </summary>
    public static void SetPreviewPair(DynamicVar var, decimal raw, decimal preview)
    {
        var.EnchantedValue = raw;
        var.PreviewValue = preview;
    }
}
