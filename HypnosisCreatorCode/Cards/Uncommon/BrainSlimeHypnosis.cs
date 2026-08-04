using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 脳くちゅ催眠 — カウント。攻撃着弾を他敵へリダイレクト（UGで相手全員へ）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BrainSlimeHypnosis() : HypnosisCreatorCard(5,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (!MassHypnosisPower.IsPropagating)
        {
            var power = await PowerCmd.Apply<BrainSlimeRedirectPower>(
                choiceContext, play.Target, 1M, Owner.Creature, this);
            if (power != null && IsUpgraded)
                power.RedirectAllEnemies = true;
        }
        else
        {
            await PowerCmd.Apply<BrainSlimeRedirectPower>(
                choiceContext, play.Target, 1M, Owner.Creature, this);
        }

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-2);
}
