using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Enchantments;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 感染 — 0コスト状態異常。手札のアタック1枚に寄生（ダメージ+3）をエンチャント。廃棄。
/// エンチャントは戦闘後も残る（本家 Sharp と同じ扱い）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Infect() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.Self)
{
    private const int ParasiteDamage = 3;

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        HoverTipFactory.FromEnchantment<Parasite>(ParasiteDamage);

    protected override bool ShouldGlowWhenConditionMet()
    {
        var hand = Owner.PlayerCombatState?.Hand;
        return hand != null && hand.Cards.Any(IsCandidate);
    }

    private bool IsCandidate(CardModel c) =>
        c != this && c.Type == CardType.Attack && c.Enchantment == null;

    protected override async Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var candidates = hand.Cards.Where(IsCandidate).ToList();
        if (candidates.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext, hand, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1),
                IsCandidate)).ToList();
        }
        // 選択できない文脈（ThrowingPlayerChoiceContext）だけ代替する。これは全端末で同じく起きる。
        // 画面の取り消し等は選ぶ本人の端末でしか起きないため握りつぶさない（片方だけ代替が走ると共闘で同期ずれ）。
        catch (NotImplementedException)
        {
            var rng = Owner.RunState.Rng.CombatCardSelection;
            selected = [candidates[rng.NextInt(candidates.Count)]];
        }

        if (selected.Count == 0) return;

        foreach (var card in selected)
            CardCmd.Enchant<Parasite>(card, ParasiteDamage);
    }
}
