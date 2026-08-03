using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カード説明／枠のダメージプレビュー用。筋力・弱体などを本家と同じ Hook.ModifyDamage で反映する。
/// </summary>
public static class CardDamagePreview
{
    /// <summary>本家 VulnerablePower の1スタックあたりの被ダメージ倍率加算（50%）。</summary>
    private const decimal VulnerableMultiplierPerStack = 0.5m;

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
            var modified = Hook.ModifyDamage(
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
            return CombatPreviewText.RoundDisplayAmount(modified);
        }
        catch
        {
            return raw;
        }
    }

    /// <summary>
    /// 自カードが先に弱体などを付与してから与えるダメージのプレビュー。
    /// 対象に <see cref="ArtifactPower"/> があるときはデバフが防がれるため、現状の補正のみ。
    /// プレビュー中に PowerCmd を同期待ちしない（ドラッグ中のフリーズ防止）。
    /// </summary>
    public static decimal ApplyAfterSelfVulnerable(
        CardModel card,
        Creature? target,
        decimal raw,
        decimal vulnerableToApply,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (vulnerableToApply <= 0)
            return ApplyModifiers(card, target, raw, props, previewMode);

        if (target is not { IsAlive: true, IsEnemy: true })
            return ApplyModifiers(card, target, raw, props, previewMode);

        if (target.GetPowerAmount<ArtifactPower>() > 0)
            return ApplyModifiers(card, target, raw, props, previewMode);

        var atCurrent = ApplyModifiers(card, target, raw, props, previewMode);
        var currentVuln = target.GetPowerAmount<VulnerablePower>();
        var oldMult = 1m + VulnerableMultiplierPerStack * currentVuln;
        var newMult = 1m + VulnerableMultiplierPerStack * (currentVuln + vulnerableToApply);
        if (oldMult <= 0m) return atCurrent;

        var preview = atCurrent * newMult / oldMult;
        return CombatPreviewText.RoundDisplayAmount(preview);
    }

    /// <summary>
    /// 本家 <see cref="DamageVar.UpdateCardPreview"/> と同様にエンチャントの加算・倍率を反映する。
    /// </summary>
    public static decimal ApplyEnchantmentModifiers(
        CardModel card,
        decimal raw,
        ValueProp props)
    {
        var enchant = card.Enchantment;
        if (enchant == null) return raw;

        try
        {
            var additive = enchant.EnchantDamageAdditive(raw, props);
            var multiplicative = enchant.EnchantDamageMultiplicative(additive, props);
            return Math.Max(multiplicative, 0m);
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

    /// <summary>
    /// カスタムプレビュー後にエンチャント付与 UI でも差分が出るよう Enchanted / Preview を揃える。
    /// </summary>
    public static void SetDamagePreviewPair(
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

        SetPreviewPair(var, rawBase, preview);
    }
}
