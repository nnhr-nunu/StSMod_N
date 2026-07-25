using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ぜんぶ知ってるよ — 性癖刺さり破滅を2倍にする。重ねがけで+1倍。UGで天賦。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class KnowItAll() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    private const decimal InitialMultiplier = 2M;
    private const decimal StackBonus = 1M;

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<KnowItAllPower>((int)InitialMultiplier)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var existing = Owner.Creature.GetPower<KnowItAllPower>();
        if (existing == null)
        {
            await PowerCmd.Apply<KnowItAllPower>(
                choiceContext, Owner.Creature, InitialMultiplier, Owner.Creature, this);
            return;
        }

        await PowerCmd.ModifyAmount(
            choiceContext, existing, StackBonus, Owner.Creature, this);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
