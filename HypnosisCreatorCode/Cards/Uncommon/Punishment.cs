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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// お仕置き — 対象がプレイヤーを攻撃した回数だけ、8ダメージの多段攻撃（UG13）。
/// 1ヒットは {Damage:diff()}、回数は戦闘中プレビュー。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Punishment() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => EnemyPlayerAttackTracker.GetCount(c) > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8M, ValueProp.Move)];

    private static int CalcHitCount(CardModel card, Creature? target) =>
        target == null ? 0 : EnemyPlayerAttackTracker.GetCount(target);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var hits = CalcHitCount(this, play.Target);
        if (hits > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hits)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5M);

    internal static void AppendDescriptionSuffix(CardModel card, Creature? target, ref string description)
    {
        if (card is not Punishment punishment) return;
        if (!CombatPreviewText.IsActive(punishment)) return;

        var previewTarget = target ?? punishment.CurrentTarget;
        var hits = CalcHitCount(punishment, previewTarget);
        if (hits <= 0) return;

        // 1ヒットの弱体等は {Damage:diff()} が緑表示。括弧は回数のみ（手計算合計は誤認しやすい）。
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（現在の攻撃回数：{hits}回）"
            : $" (Current attacks: {hits})";
        CombatPreviewText.AppendSuffix(punishment, ref description, suffix);
    }
}
