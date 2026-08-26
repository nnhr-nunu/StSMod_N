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
/// 連続指パッチン — 全敵に1ダメージ×4をX回。廃棄。UGで保留・5連撃。
/// 合計ダメージは戦闘中のみ表示し、筋力／弱体などを反映する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteFingerSnap() : HypnosisCreatorCard(-1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    private const int BaseHitsPerCycle = 4;
    private const int UpgradedHitsPerCycle = 5;

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var x = Math.Max(0, ResolveEnergyXValue());
        var totalHits = GetHitsPerCycle(this) * x;
        if (totalHits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(totalHits)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);

    private static int GetHitsPerCycle(InfiniteFingerSnap card) =>
        card.IsUpgraded ? UpgradedHitsPerCycle : BaseHitsPerCycle;

    internal static void AppendDescriptionSuffix(
        CardModel card, Creature? target, ref string description)
    {
        if (card is not InfiniteFingerSnap snap) return;
        if (!CombatPreviewText.IsActive(snap)) return;

        var previewX = EnergyXPreview.Resolve(snap);
        var baselineX = EnergyXPreview.ResolveBaseline(snap);
        var hits = GetHitsPerCycle(snap) * previewX;
        var baselineHits = BaseHitsPerCycle * baselineX;
        if (hits <= 0) return;

        var perHitRaw = snap.DynamicVars.Damage.BaseValue;
        var perHitPreview = CombatDamageSuffixPreview.ResolveAoEPerHit(snap, perHitRaw, ValueProp.Move);
        var total = CombatDamageSuffixPreview.SumRepeatedHits(perHitPreview, hits);
        var baselineTotal = CombatDamageSuffixPreview.SumRepeatedHits(perHitRaw, baselineHits);

        CombatDamageSuffixPreview.AppendTotalDamageSuffix(snap, ref description, total, baselineTotal);
    }
}
