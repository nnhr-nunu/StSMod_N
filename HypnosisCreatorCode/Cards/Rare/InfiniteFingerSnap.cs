using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 連続指パッチン — 全敵に1ダメージ×5をX回。廃棄。UGで保留。
/// 合計ダメージは戦闘中のみ表示し、筋力／弱体などを反映する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteFingerSnap() : HypnosisCreatorCard(-1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    private const int HitsPerCycle = 5;

    static InfiniteFingerSnap()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendTotalDamageWhenPlayable;
    }

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var x = Math.Max(0, ResolveEnergyXValue());
        var totalHits = HitsPerCycle * x;
        if (totalHits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(totalHits)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);

    private static void AppendTotalDamageWhenPlayable(
        CardModel card, Creature? target, ref string description)
    {
        if (card is not InfiniteFingerSnap snap) return;
        if (!CombatPreviewText.IsActive(snap)) return;

        var x = Math.Max(0, snap.ResolveEnergyXValue());
        var hits = HitsPerCycle * x;
        if (hits <= 0) return;

        var previewTarget = target
            ?? snap.CombatState!.HittableEnemies.FirstOrDefault(e => e.IsAlive && e.IsEnemy);
        var perHitRaw = snap.DynamicVars.Damage.BaseValue;
        var perHit = CardDamagePreview.ApplyModifiers(snap, previewTarget, perHitRaw, ValueProp.Move);
        var total = perHit * hits;
        if (total <= 0) return;

        var formatted = FormatDamage(total);
        // 弱体・筋力で1ヒットが変わったときだけ緑（:diff() と同じ見た目）
        if (perHit != perHitRaw)
            formatted = $"[green]{formatted}[/green]";

        var totalText = UpgradeCardText.IsJapaneseUi()
            ? $"（合計{formatted}ダメージ）"
            : $" (Total {formatted} damage)";
        CombatPreviewText.AppendSuffix(snap, ref description, totalText);
    }

    private static string FormatDamage(decimal amount)
    {
        var culture = LocManager.Instance?.CultureInfo;
        return amount == decimal.Truncate(amount)
            ? ((int)amount).ToString(culture)
            : amount.ToString("0.##", culture);
    }
}
