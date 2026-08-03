using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 時止めストライクの遅延ダメージ — プレビューとターン終了時の実効を同じ Hook 経路で揃える。
/// 弱体・感度3000倍（SensitivityPower）・筋力などは蓄積後の一括ダメージに乗る。
/// </summary>
public static class TimeStopStrikeDamage
{
    public static decimal ResolvePreview(
        CardModel card,
        Creature? target,
        decimal stackedBase,
        CardPreviewMode previewMode = CardPreviewMode.Normal) =>
        CardDamagePreview.ApplyModifiers(card, target, stackedBase, ValueProp.Move, previewMode);

    public static decimal ResolveTurnEnd(
        IRunState runState,
        ICombatState combat,
        Creature target,
        Creature dealer,
        decimal stackedBase)
    {
        try
        {
            return CombatPreviewText.RoundDisplayAmount(
                Hook.ModifyDamage(
                    runState,
                    combat,
                    target,
                    dealer,
                    stackedBase,
                    ValueProp.Move,
                    cardSource: null!,
                    cardPlay: null,
                    ModifyDamageHookType.All,
                    CardPreviewMode.None,
                    out _));
        }
        catch
        {
            return stackedBase;
        }
    }

    public static Creature? ResolveDealer(Creature? applier, Creature? markOwner)
    {
        if (applier is { IsPlayer: true }) return applier;
        return markOwner?.CombatState?.Players.FirstOrDefault()?.Creature;
    }
}
