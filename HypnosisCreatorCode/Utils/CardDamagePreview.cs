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

        var owner = card.Owner;
        if (owner?.Creature == null)
            return ApplyModifiers(card, target, raw, props, previewMode);

        if (target is not { IsAlive: true, IsEnemy: true })
            return ApplyModifiers(card, target, raw, props, previewMode);

        if (target.GetPowerAmount<ArtifactPower>() > 0)
            return ApplyModifiers(card, target, raw, props, previewMode);

        var vuln = target.GetPower<VulnerablePower>();
        var hadVuln = vuln != null;
        var createdTemp = false;

        try
        {
            if (hadVuln)
            {
                PowerCmd.ModifyAmount(
                    new ThrowingPlayerChoiceContext(),
                    vuln!,
                    vulnerableToApply,
                    owner.Creature,
                    card,
                    silent: true).GetAwaiter().GetResult();
            }
            else
            {
                PowerCmd.Apply<VulnerablePower>(
                    new ThrowingPlayerChoiceContext(),
                    target,
                    vulnerableToApply,
                    owner.Creature,
                    card,
                    silent: true).GetAwaiter().GetResult();
                createdTemp = true;
            }

            return ApplyModifiers(card, target, raw, props, previewMode);
        }
        catch
        {
            return raw;
        }
        finally
        {
            try
            {
                if (createdTemp)
                {
                    var temp = target.GetPower<VulnerablePower>();
                    if (temp != null)
                        PowerCmd.ModifyAmount(
                            new ThrowingPlayerChoiceContext(),
                            temp,
                            -vulnerableToApply,
                            owner.Creature,
                            card,
                            silent: true).GetAwaiter().GetResult();
                }
                else if (hadVuln && vuln != null)
                {
                    PowerCmd.ModifyAmount(
                        new ThrowingPlayerChoiceContext(),
                        vuln,
                        -vulnerableToApply,
                        owner.Creature,
                        card,
                        silent: true).GetAwaiter().GetResult();
                }
            }
            catch
            {
                // プレビュー復元失敗は戦闘実効に影響しない
            }
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
