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

/// <summary>後悔 — 手札1枚につきHP1失わせる。廃棄。アブノーマル。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class RegretCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    public override string PortraitPath => "regret.png".CardImagePath();
    public override string CustomPortraitPath => "regret.png".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var count = Owner.PlayerCombatState?.Hand?.Cards.Count ?? 0;
        if (count > 0)
        {
            await CreatureCmd.Damage(
                choiceContext, play.Target, count,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                Owner.Creature, this, play);
        }
        await ResolveFetishOnTarget(choiceContext, play);
    }

}
