using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 脳くちゅ催眠 — カウント・アブノーマル。
/// CSV: 本来は対象の攻撃意図を(対象を含む)ランダムな敵へ向けさせる効果を想定。
/// リダイレクトAPIが未確定のため、暫定で混乱(Confused)+トランスで近似する。UGで敵全体に混乱を付与。
/// TODO: リダイレクトAPIが確定したら本実装に差し替える。
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
                await PowerCmd.Apply<ConfusedPower>(
                    choiceContext, enemy, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<ConfusedPower>(
                choiceContext, play.Target, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);
        }

        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() { }
}
