using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>解剖 — 14＋心臓数×4（UG×7）。リーサルで追加レリック報酬。廃棄なし。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Autopsy() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    static Autopsy()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendDamagePreview;
    }

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(14M),
        new ExtraDamageVar(4M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(HeartCountMultiplier)
    ];

    /// <summary>
    /// 心臓数はラン進行データ。本家 CalculatedVar.Calculate は card.CombatState 未設定時に倍率 0 になるため、
    /// AutopsyPreviewPatch からも呼ぶ。
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

        // リーサル時は報酬画面の追加レリックへ（心停止＋・未UGの心臓えぐり出しと同じ）
        if (play.Target is { IsAlive: false })
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(3M); // 4 → 7

    private static decimal HeartCountMultiplier(CardModel card, Creature? target) =>
        HeartInventory.CountHearts(card.Owner);

    internal decimal PreviewModifiedDamage(Creature? target, CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        var raw = ComputeHeartScaledDamage();
        var owner = Owner;
        if (owner?.Creature == null) return raw;

        // 手札ホバー時は card.CombatState が null のことがある（本家 CalculatedDamageVar と同じフォールバック）
        var combat = CombatState ?? owner.Creature.CombatState;
        if (combat == null) return raw;

        try
        {
            return Hook.ModifyDamage(
                owner.RunState,
                combat,
                target,
                owner.Creature,
                raw,
                ValueProp.Move,
                this,
                cardPlay: null,
                ModifyDamageHookType.All,
                previewMode,
                out _);
        }
        catch
        {
            return raw;
        }
    }

    private static void AppendDamagePreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not Autopsy autopsy) return;
        if (!CombatPreviewText.IsActive(autopsy)) return;

        var total = autopsy.PreviewModifiedDamage(target ?? autopsy.CurrentTarget);
        if (total <= autopsy.DynamicVars.CalculationBase.BaseValue) return;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（{FormatDamage(total)}ダメージ）"
            : $" ({FormatDamage(total)} damage)";
        CombatPreviewText.AppendSuffix(autopsy, ref description, suffix);
    }

    private static string FormatDamage(decimal amount)
    {
        var culture = LocManager.Instance?.CultureInfo;
        return amount == decimal.Truncate(amount)
            ? ((int)amount).ToString(culture)
            : amount.ToString("0.##", culture);
    }
}
