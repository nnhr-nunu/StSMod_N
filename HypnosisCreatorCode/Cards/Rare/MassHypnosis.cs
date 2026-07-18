using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>集団催眠 — パワー。単体対象のカウントカードをプレイすると、破滅が他の敵全員へ延焼する。UGでInnate。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MassHypnosis() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<MassHypnosisPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
