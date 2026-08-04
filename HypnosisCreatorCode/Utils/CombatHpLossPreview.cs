using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 攻撃リーサル判定用。本家 CreatureCmd.Damage と同趣旨の3段
/// （ModifyDamage → Block 減算 → ModifyHpLost）。
/// Hook.ModifyDamage は All フェーズ（脱力・飛行50%減・霊体Cap・不死身Cap 等を含む）。
/// </summary>
public static class CombatHpLossPreview
{
    /// <summary>
    /// 段1済みの与ダメージから、ブロック・HP喪失フック後の実際の HP 減少量を返す。
    /// </summary>
    public static decimal ComputeHpLossFromAttackDamage(
        CardModel card,
        Creature target,
        decimal effectiveDamageAfterModifyDamage,
        ValueProp props)
    {
        if (!target.IsAlive) return 0m;

        var unblocked = Math.Max(0m, effectiveDamageAfterModifyDamage - target.Block);
        if (unblocked <= 0) return 0m;

        return ApplyHpLossHooks(card, target, unblocked, props);
    }

    public static decimal ApplyHpLossHooks(
        CardModel card,
        Creature target,
        decimal unblockedDamage,
        ValueProp props)
    {
        var owner = card.Owner;
        if (owner?.Creature == null)
            return CombatPreviewText.RoundDisplayAmount(unblockedDamage);

        var combat = card.CombatState ?? owner.Creature.CombatState;
        if (combat == null)
            return CombatPreviewText.RoundDisplayAmount(unblockedDamage);

        if (!CardDamagePreview.ShouldUseCombatDamageHooks(card, runGlobalHooks: true))
            return CombatPreviewText.RoundDisplayAmount(unblockedDamage);

        try
        {
            var modified = Hook.ModifyHpLost(
                owner.RunState,
                combat,
                target,
                unblockedDamage,
                props,
                owner.Creature,
                card,
                HpLossHookPhase.All,
                out _);
            return CombatPreviewText.RoundDisplayAmount(modified);
        }
        catch
        {
            return CombatPreviewText.RoundDisplayAmount(unblockedDamage);
        }
    }

    public static bool WouldDamageKill(Creature target, decimal hpLoss) =>
        target.IsAlive && hpLoss >= target.CurrentHp;
}
