using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// ミラーリング — 相手の攻撃予定と同じ攻撃。廃棄。
/// 数値は相手と同じ。こちら側の筋力は反映しない。弱体は敵デバフなので有効（CSV備考）。
/// 多段は WithHitCount 1回で解決。プレビューは IntentDerivedPreviewPatch。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mirroring() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // Unpowered: プレイヤー筋力を載せない
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, ValueProp.Unpowered)];

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    public static bool HasAttackIntent(Creature target) =>
        EnemyAttackIntents.IntendsToAttack(target);

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(HasAttackIntent);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!EnemyAttackIntents.TryGetPerHit(play.Target, out var damage, out var hits))
            return;

        DynamicVars.Damage.BaseValue = damage;
        if (hits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
    }
}
