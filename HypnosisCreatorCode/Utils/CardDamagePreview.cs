using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カード説明／枠のダメージプレビュー用。筋力・弱体などを本家と同じ Hook.ModifyDamage で反映する。
/// </summary>
public static class CardDamagePreview
{
    /// <summary>本家 VulnerablePower の1スタックあたりの被ダメージ倍率加算（50%）。</summary>
    private const decimal VulnerableMultiplierPerStack = 0.5m;

    /// <summary>
    /// 本家 <see cref="Hook.ModifyDamage"/> 経路。呼び出し側はカード素のダメージを渡すこと
    /// （エンチャント加算は Hook 内。神秘のライター +9 等を先に足すと二重になる）。
    /// </summary>
    public static decimal ApplyModifiers(
        CardModel card,
        Creature? target,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true)
    {
        if (!ShouldUseCombatDamageHooks(card, runGlobalHooks))
            return ApplyEnchantmentModifiers(card, raw, props);

        var owner = card.Owner;
        if (owner?.Creature == null)
            return ApplyEnchantmentModifiers(card, raw, props);

        var combat = card.CombatState ?? owner.Creature.CombatState;
        if (combat == null)
            return ApplyEnchantmentModifiers(card, raw, props);

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
            return ApplyEnchantmentModifiers(card, raw, props);
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
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true)
    {
        if (vulnerableToApply <= 0)
            return ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);

        if (target is not { IsAlive: true, IsEnemy: true })
            return ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);

        if (target.GetPowerAmount<ArtifactPower>() > 0)
            return ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);

        var dealer = card.Owner?.Creature;
        if (dealer == null)
            return ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);

        var atCurrent = ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);
        var currentVuln = target.GetPowerAmount<VulnerablePower>();
        var oldFactor = ResolvePoweredAttackVulnerableFactor(
            target, dealer, props, card, currentVuln);
        var newFactor = ResolvePoweredAttackVulnerableFactor(
            target, dealer, props, card, currentVuln + (int)vulnerableToApply);
        if (oldFactor <= 0m) return atCurrent;

        var preview = atCurrent * newFactor / oldFactor;
        return CombatPreviewText.RoundDisplayAmount(preview);
    }

    /// <summary>
    /// 本家 <see cref="VulnerablePower.ModifyDamageMultiplicative"/> と同趣旨の弱体系倍率。
    /// 無慈悲・紙カエル（Paper Phrog）・衰弱（Debilitate）を弱体あり時に反映する。
    /// </summary>
    internal static decimal ResolvePoweredAttackVulnerableFactor(
        Creature target,
        Creature dealer,
        ValueProp props,
        CardModel? cardSource,
        int vulnerableStacks)
    {
        if (vulnerableStacks <= 0) return 1m;
        if (!props.IsPoweredAttack()) return 1m;

        var stackMult = 1m + VulnerableMultiplierPerStack * vulnerableStacks;

        // 本家 VulnerablePower の DamageIncrease 既定値
        var vulnComponent = 1.5m;
        var player = dealer.Player;
        if (player?.GetRelic<PaperPhrog>() is { } phrog)
            vulnComponent = phrog.ModifyVulnerableMultiplier(
                target, vulnComponent, props, dealer, cardSource);

        var cruelty = dealer.GetPower<CrueltyPower>()
            ?? dealer.PetOwner?.Creature.GetPower<CrueltyPower>();
        if (cruelty != null)
            vulnComponent = cruelty.ModifyVulnerableMultiplier(
                target, vulnComponent, props, dealer, cardSource);

        var debilitate = target.GetPower<DebilitatePower>();
        if (debilitate != null)
            vulnComponent = debilitate.ModifyVulnerableMultiplier(
                target, vulnComponent, props, dealer, cardSource);

        return stackMult * (vulnComponent / 1.5m);
    }

    /// <summary>
    /// イベントのエンチャント付与プレビュー等では本家も戦闘フックを走らせない。
    /// ここで誤って <see cref="Hook.ModifyDamage"/> すると 15→1 のような表示になる。
    /// </summary>
    public static bool ShouldUseCombatDamageHooks(CardModel card, bool runGlobalHooks)
    {
        if (!runGlobalHooks) return false;
        if (card.IsEnchantmentPreview) return false;
        if (EnchantPreviewUiGuard.IsActive) return false;
        if (card.PreviewOutsideOfCombat) return false;

        var owner = card.Owner;
        if (owner?.Creature == null) return false;

        return (card.CombatState ?? owner.Creature.CombatState) != null;
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
    /// 本家 DamageVar と同様。付与確認は素値→エンチャント後。戦闘中は弱体などでプレビューが下がってもそのまま出す。
    /// </summary>
    public static void SetDamagePreviewPair(
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
    /// 戦闘フックが使えるときは本家 Hook（エンチャント込み）。使えないときはエンチャントのみ。先にエンチャントしてから Hook に渡さない。
    /// </summary>
    public static decimal ResolvePreview(
        CardModel card,
        Creature? target,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true) =>
        ApplyModifiers(card, target, raw, props, previewMode, runGlobalHooks);
}
