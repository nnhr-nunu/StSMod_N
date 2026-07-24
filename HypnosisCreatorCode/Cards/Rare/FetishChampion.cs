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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 性癖の覇者 — 対象の性癖数だけ20ダメージを与える（UG25）。
/// 1ヒットは {Damage:diff()}、回数は戦闘中プレビュー。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishChampion() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Abnormal, FetishType.Sm, FetishType.DomSub];

    public override bool? FetishHitPerTypeOverride => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(20M, ValueProp.Move)];

    private static int CalcHitCount(CardModel card, Creature? target) =>
        target == null ? 0 : FetishCombat.GetFetishes(target).Count;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hits = CalcHitCount(this, play.Target);
        if (hits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5M);

    internal static void AppendDescriptionSuffix(CardModel card, Creature? target, ref string description)
    {
        if (card is not FetishChampion champion) return;
        if (!CombatPreviewText.IsActive(champion)) return;

        var previewTarget = target ?? champion.CurrentTarget;
        var hits = CalcHitCount(champion, previewTarget);
        if (hits <= 0) return;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（攻撃回数：{hits}回）"
            : $" ({hits} hits)";
        CombatPreviewText.AppendSuffix(champion, ref description, suffix);
    }
}
