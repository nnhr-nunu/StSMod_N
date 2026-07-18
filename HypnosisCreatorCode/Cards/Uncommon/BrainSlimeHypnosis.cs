using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 脳くちゅ催眠 — カウント・アブノーマル。
/// 攻撃着弾を他の敵へリダイレクト（味方除く）。API限界時は Confused も併用。
/// UGで敵全体にリダイレクト＋混乱。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BrainSlimeHypnosis() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ConfusedPower>(1M),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (IsUpgraded && CombatState != null)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
            {
                await PowerCmd.Apply<BrainSlimeRedirectPower>(
                    choiceContext, enemy, 1M, Owner.Creature, this);
                await PowerCmd.Apply<ConfusedPower>(
                    choiceContext, enemy, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);
            }
        }
        else
        {
            await PowerCmd.Apply<BrainSlimeRedirectPower>(
                choiceContext, play.Target, 1M, Owner.Creature, this);
            await PowerCmd.Apply<ConfusedPower>(
                choiceContext, play.Target, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);
        }

        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() { }
}
