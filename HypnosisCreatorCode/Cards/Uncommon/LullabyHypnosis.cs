using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>寝かしつけ催眠 — カウント。対象を睡眠させ、対象のHPを回復し、トランスを付与する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class LullabyHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AsleepPower>(1M),
        new DynamicVar("Heal", 20M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<AsleepPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<AsleepPower>(
            choiceContext, play.Target, DynamicVars["AsleepPower"].BaseValue, Owner.Creature, this);
        await ForcedSleep.EnsurePresentation(choiceContext, play.Target, Owner.Creature, this);
        await CreatureCmd.Heal(play.Target, DynamicVars["Heal"].BaseValue);
        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AsleepPower"].UpgradeValueBy(1M);
        DynamicVars["Heal"].UpgradeValueBy(10M);
    }
}
