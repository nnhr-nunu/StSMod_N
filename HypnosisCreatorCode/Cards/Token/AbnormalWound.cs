using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 負傷 — 状態異常。10ダメージ。廃棄。
/// 本家 Wound の状態異常催眠置き換え先／苦痛の一刺し付与。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalWound() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Unpowered)];

    protected override async Task PlayStatusEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars.Damage.BaseValue,
            ValueProp.Unpowered, Owner.Creature, null, null);
    }
}
