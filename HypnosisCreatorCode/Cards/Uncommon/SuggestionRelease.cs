using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 暗示解除 — ダメージ無し。対象のトランスを全て解除し、解除した分だけエナジーを得る。廃棄。
/// UGでは解除した分だけカードも引く。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SuggestionRelease() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var stacks = TranceCombat.GetTrance(play.Target);
        if (stacks <= 0) return;

        var trance = play.Target.GetPower<TrancePower>();
        if (trance != null) await PowerCmd.Remove(trance);

        await PlayerCmd.GainEnergy(stacks, Owner);
        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, stacks, Owner);
    }

    protected override void OnUpgrade() { }
}
