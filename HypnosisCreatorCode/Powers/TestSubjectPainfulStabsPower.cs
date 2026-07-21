using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 苦痛の一刺し（実験体）— ブロックされずにダメージを与えるたび、プレイ可能な負傷を Amount 枚手札に加える。
/// </summary>
public class TestSubjectPainfulStabsPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "test_subject_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "test_subject_heart.png".BigRelicImagePath();

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature? target,
        CardModel? cardSource)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (dealer != Owner) return;
        if (result.UnblockedDamage <= 0) return;
        if (Amount <= 0) return;
        if (Owner.Player == null) return;

        Flash();
        await StatusHypnosisConvert.AddFreePlayableAsync<AbnormalWound>(
            Owner.Player, Amount, PileType.Hand);
    }
}
