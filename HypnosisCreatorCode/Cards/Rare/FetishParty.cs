using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 性癖パーリナイ — マルチ用。味方全員の手札にランダムな性癖カードを加える。
/// ソロプレイでは自分のみが対象になる。
/// TODO: マルチプレイ環境での動作確認が必要（mechanics-lock.md「マルチ用」参照）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishParty() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    private static bool IsFetishCard(CardModel c) =>
        c is HypnosisCreatorCard { CardFetishes.Count: > 0 }
        && c.Rarity != CardRarity.Token
        && c.Type is not CardType.Status and not CardType.Curse;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var pool = ModelDb.AllCards.Where(IsFetishCard).ToList();
        if (pool.Count == 0) return;

        foreach (var player in CombatState.Players)
        {
            var rng = player.RunState.Rng.CombatCardSelection;
            var generated = new List<CardModel>(DynamicVars.Cards.IntValue);
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                var canonical = pool[rng.NextInt(pool.Count)];
                generated.Add(CombatState.CreateCard(canonical, player));
            }

            await CombatCardPilePreview.AddGeneratedCardsAsync(generated, PileType.Hand, player);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
