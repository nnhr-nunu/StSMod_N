using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 自己暗示 — パワー。筋力とスピードを得て、心臓レリックを複数入手する（この戦闘限定のスタックとして扱う）。
/// CSV: 「この戦闘中だけ」の心臓は永続レリックシステムと分離できないため、暫定で通常の StolenHeart 入手として実装する。
/// TODO: 戦闘限定スタックのレリック表現が確定したら差し替える。
/// UGでより多くの筋力・スピードを得る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SelfSuggestion() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1M),
        new PowerVar<DexterityPower>(1M),
        new DynamicVar("Hearts", 3M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(choiceContext, self, DynamicVars["StrengthPower"].BaseValue, self, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, DynamicVars["DexterityPower"].BaseValue, self, this);

        for (var i = 0; i < DynamicVars["Hearts"].IntValue; i++)
            await RelicCmd.Obtain<StolenHeart>(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1M);
        DynamicVars["DexterityPower"].UpgradeValueBy(1M);
    }
}
