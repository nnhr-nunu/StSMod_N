using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using HypnosisCreator.HypnosisCreatorCode.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>デブリ — 5ダメージ。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DebrisStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => "debris.png".CardImagePath();
    public override string CustomPortraitPath => "debris.png".BigCardImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Unpowered)];

    protected override async Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, this, play);
    }
}
