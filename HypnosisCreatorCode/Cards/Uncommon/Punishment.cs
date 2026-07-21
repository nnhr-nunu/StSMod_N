using BaseLib.Patches.Localization;
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
/// 多段は WithHitCount 1回で解決（手動ループ禁止）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Punishment() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    static Punishment()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendHitPreview;
    }

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => EnemyPlayerAttackTracker.GetCount(c) > 0);

    // FetishChampion と同様、CalculatedDamageVar+CalculatedVar 併用を避ける。
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

    private static void AppendHitPreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not Punishment punishment) return;
        if (!CombatPreviewText.IsActive(punishment)) return;

        var previewTarget = target ?? punishment.CurrentTarget;
        var hits = CalcHitCount(punishment, previewTarget);
        var perHit = punishment.DynamicVars.Damage.BaseValue;
        var total = perHit * hits;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（攻撃回数：{hits}回／{total}ダメージ）"
            : $" ({hits} hits / {total} damage)";
        CombatPreviewText.AppendSuffix(punishment, ref description, suffix);
    }
}
