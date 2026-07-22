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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>解剖 — 14＋心臓数×4（UG×7）。リーサルで追加レリック報酬。廃棄。</summary>
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

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14M, ValueProp.Move),
        new DynamicVar("PerHeart", 4M)
    ];

    protected override bool ShouldGlowWhenConditionMet() =>
        HeartInventory.CountHearts(Owner) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hearts = HeartInventory.CountHearts(Owner);
        var damage = DynamicVars.Damage.BaseValue + hearts * DynamicVars["PerHeart"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        // リーサル時は報酬画面の追加レリックへ（心停止＋・未UGの心臓えぐり出しと同じ）
        if (play.Target is { IsAlive: false })
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["PerHeart"].UpgradeValueBy(3M); // 4 → 7

    private static void AppendDamagePreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not Autopsy autopsy) return;

        var hearts = HeartInventory.CountHearts(autopsy.Owner);
        var total = autopsy.DynamicVars.Damage.BaseValue + hearts * autopsy.DynamicVars["PerHeart"].BaseValue;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（心臓{hearts}個／{total}ダメージ）"
            : $" ({hearts} Hearts / {total} damage)";

        if (description.Contains(suffix, StringComparison.Ordinal)) return;
        description = description.TrimEnd() + suffix;
    }
}
