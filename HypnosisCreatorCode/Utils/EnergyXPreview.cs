using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Xコストカードの戦闘中プレビュー用。本家 <see cref="CardModel.ResolveEnergyXValue" /> は
/// <see cref="MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.CapturedXValue" />（プレイ後に確定）を使うため、
/// ホバー時は残エナジー＋ケミカルX等を反映できない。
/// </summary>
public static class EnergyXPreview
{
    public static int Resolve(CardModel card)
    {
        if (!card.EnergyCost.CostsX || card.CombatState == null) return 0;

        var energyToSpend = Math.Max(0, card.EnergyCost.GetAmountToSpend());
        return Math.Max(0, Hook.ModifyXValue(card.CombatState, card, energyToSpend));
    }

    public static int ResolveBaseline(CardModel card) =>
        !card.EnergyCost.CostsX ? 0 : Math.Max(0, card.EnergyCost.GetAmountToSpend());
}
