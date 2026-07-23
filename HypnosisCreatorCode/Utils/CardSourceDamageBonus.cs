using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 遅延ダメージの蓄積など、プレイ時にカード source が要る加算修正（ストライクダミー等）を取り込む。
/// 筋力など source 不要の加算はターン終了時の <see cref="CreatureCmd.Damage"/> 側に任せる。
/// </summary>
public static class CardSourceDamageBonus
{
    public static decimal AmountToStack(
        CardModel card,
        Creature target,
        CardPlay play,
        decimal baseDamage,
        ValueProp props)
    {
        var owner = card.Owner;
        if (owner == null) return baseDamage;
        var dealer = owner.Creature;
        if (dealer == null) return baseDamage;

        var combat = card.CombatState ?? dealer.CombatState;
        if (combat == null) return baseDamage;

        try
        {
            var withoutCard = Hook.ModifyDamage(
                owner.RunState,
                combat,
                target,
                dealer,
                baseDamage,
                props,
                cardSource: null!,
                cardPlay: null,
                ModifyDamageHookType.Additive,
                CardPreviewMode.None,
                out _);

            var withCard = Hook.ModifyDamage(
                owner.RunState,
                combat,
                target,
                dealer,
                baseDamage,
                props,
                card,
                play,
                ModifyDamageHookType.Additive,
                CardPreviewMode.None,
                out _);

            return baseDamage + (withCard - withoutCard);
        }
        catch
        {
            return baseDamage;
        }
    }
}
