using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// 初心者向け催眠 — カウント。破滅12＋次の性癖カードのタグを植え付け＋トランス1。
/// 集団催眠で波及した対象も Arm に積み上げ、次の性癖カードで全員へ植え付ける。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BeginnerHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Doom", 12M),
        new DynamicVar("Trance", 1M),
        new DynamicVar("PlantCards", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);

        await FetishPlantPending.Arm(
            choiceContext, Owner, play.Target, DynamicVars["PlantCards"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["PlantCards"].UpgradeValueBy(1M);
}
