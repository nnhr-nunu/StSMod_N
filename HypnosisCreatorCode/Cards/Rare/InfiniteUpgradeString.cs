using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 糸色丁頁 — カウント。対象はHP15を失う。トランス1。無限UGで失HP+7。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteUpgradeString() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override int MaxUpgradeLevel => int.MaxValue;
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("LoseHp", 15M),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars["LoseHp"].BaseValue, ValueProp.Move, Owner.Creature, this, play);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["LoseHp"].UpgradeValueBy(7M);
}
