using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// ミラーリング — 相手の攻撃予定と同じ攻撃（同じ1ヒット値・同じ連撃数）。廃棄。
/// 既定は意図表示値をそのまま与え、プレイヤー側の状態異常補正は載せない。
/// 旧補正付き挙動は <see cref="MirroringDamagePreview.UseIntentValuesOnly"/> で復帰可能。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mirroring() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, MirroringRules.DamageProps)];

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    internal static void AppendDescriptionSuffix(CardModel card, Creature? target, ref string description)
    {
        if (card is not Mirroring mirroring) return;

        var previewTarget = target ?? mirroring.CurrentTarget;
        if (previewTarget == null || !EnemyAttackIntents.TryGetPerHit(previewTarget, out var damage, out _))
            return;
        if (damage <= 0) return;

        var preview = MirroringDamagePreview.Resolve(mirroring, previewTarget, damage);
        CombatDamageSuffixPreview.AppendCompactDealDamageSuffix(
            mirroring, ref description, preview, damage);
    }

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
