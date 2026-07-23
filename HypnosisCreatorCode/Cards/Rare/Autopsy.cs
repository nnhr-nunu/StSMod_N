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
/// 解剖 — 14＋心臓数×4（UG×7）。リーサルで追加レリック報酬。廃棄なし。
/// 合計ダメージのプレビューは説明文の {CalculatedDamage:diff()} と枠表示。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Autopsy() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(14M),
        new ExtraDamageVar(4M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(HeartCountMultiplier)
    ];

    /// <summary>
    /// 心臓数はラン進行データ。本家 CalculatedVar.Calculate は card.CombatState 未設定時に倍率 0 になるため、
    /// HypnosisCalculatedDamagePreviewPatch からも呼ぶ。
    /// </summary>
    internal decimal ComputeHeartScaledDamage()
    {
        if (Owner == null) return DynamicVars.CalculationBase.BaseValue;
        var hearts = HeartInventory.CountHearts(Owner);
        return DynamicVars.CalculationBase.BaseValue + DynamicVars.ExtraDamage.BaseValue * hearts;
    }

    protected override bool ShouldGlowWhenConditionMet() =>
        HeartInventory.CountHearts(Owner) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        if (play.Target is { IsAlive: false })
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(3M); // 4 → 7

    private static decimal HeartCountMultiplier(CardModel card, Creature? _) =>
        HeartInventory.CountHearts(card.Owner);
}
