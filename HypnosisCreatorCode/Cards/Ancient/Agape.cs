using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Ancient;

/// <summary>
/// Agape（No.107）— 古代スキル。調和の超越先（古代の牙）。
/// 相手の攻撃合計と同値のブロック＋次2ターンのターン開始時も同量。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Agape() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    private const int TurnStartBlockCount = 2;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override string PortraitPath => "harmony.png".CardImagePath();
    public override string CustomPortraitPath => "harmony.png".BigCardImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(0M, ValueProp.Unpowered)];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => EnemyAttackIntents.GetTotalDamage(c) > 0);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var block = EnemyAttackIntents.GetTotalDamage(play.Target);
        if (block <= 0) return;

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, play);

        var power = await PowerCmd.Apply<AgapePower>(
            choiceContext, Owner.Creature, block, Owner.Creature, this);
        if (power != null)
            power.TurnsRemaining = TurnStartBlockCount;
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
