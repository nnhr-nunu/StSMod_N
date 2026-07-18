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
/// 糸色丁頁 — カウント。SM。自身のHPを失いトランス1を付与する。
/// 無限アップグレード可能で、アップグレードごとに失うHPが+7され、カード名に「+N」が表示される。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteUpgradeString() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override int MaxUpgradeLevel => int.MaxValue;
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("LoseHp", 15M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, Owner.Creature, DynamicVars["LoseHp"].BaseValue, ValueProp.Move, null, this, play);
        await TranceCombat.ApplyTrance(choiceContext, play.Target, 1, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["LoseHp"].UpgradeValueBy(7M);
}
