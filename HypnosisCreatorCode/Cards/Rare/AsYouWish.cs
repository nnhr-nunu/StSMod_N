using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ご主人様の言うとおり — パワー。性癖タグ付きカードをプレイするたび、タグの種類に応じて自己強化する。
/// アブノーマル→筋力、SM→活力、DomSub→ブロック。UGで数値上昇。重ねがけは寄与を合算。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AsYouWish() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<AsYouWishPower>(
            choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() { }
}
