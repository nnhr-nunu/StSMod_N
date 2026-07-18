using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 剥き出しの本能 — CSV: 本来は CardCmd.Enchant で手札カードに「本能」付与効果を予定。
/// エンチャントAPI・「本能」キーワードの仕様が未確定のため、暫定でカードを2枚引く効果に差し替え。
/// TODO: sts2 側の CardCmd.Enchant / Instinct 系実装が確定したら置き換える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BareInstinct() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
