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

/// <summary>
/// 連続指パッチン — 全敵に1ダメージ×5をX回。廃棄。UGで保留。
/// 合計ダメージは戦闘中（プレイ可能時）のみ表示し、筋力／弱体などを反映する。
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

        // 本家 Whirlwind と同様、1回の Attack の HitCount にまとめて解決する
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(totalHits)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);

    /// <summary>
    /// （合計Nダメージ）は戦闘中かつ X&gt;0 のときだけ差し込む。
    /// ヒットごとの ModifyDamage（筋力・弱体）×ヒット数で実ダメージに合わせる。
    /// </summary>
    private static void AppendTotalDamageWhenPlayable(
        CardModel card, Creature? target, ref string description)
    {
        if (card is not InfiniteFingerSnap snap) return;
        if (snap.CombatState == null) return;

        var x = Math.Max(0, snap.ResolveEnergyXValue());
        var hits = HitsPerCycle * x;
        if (hits <= 0) return;

        // 全体攻撃: 照準中ならその敵、なければ先頭の敵で弱体などを反映
        var previewTarget = target
            ?? snap.CombatState.HittableEnemies.FirstOrDefault(e => e.IsAlive && e.IsEnemy);
        var perHit = PreviewModifiedDamage(snap, previewTarget);
        var total = perHit * hits;
        if (total <= 0) return;

        var totalText = UpgradeCardText.IsJapaneseUi()
            ? $"（合計{FormatDamage(total)}ダメージ）"
            : $" (Total {FormatDamage(total)} damage)";

        if (description.Contains(totalText, StringComparison.Ordinal)) return;

        // 「廃棄。」／"Exhaust." の直前に挿入
        if (UpgradeCardText.IsJapaneseUi())
        {
            description = description.Contains("廃棄。", StringComparison.Ordinal)
                ? description.Replace("廃棄。", totalText + "廃棄。", StringComparison.Ordinal)
                : description.TrimEnd() + totalText;
            return;
        }

        description = description.Contains("Exhaust.", StringComparison.OrdinalIgnoreCase)
            ? description.Replace("Exhaust.", totalText.TrimStart() + " Exhaust.", StringComparison.OrdinalIgnoreCase)
            : description.TrimEnd() + totalText;
    }

    private static decimal PreviewModifiedDamage(InfiniteFingerSnap card, Creature? target)
    {
        var amount = card.DynamicVars.Damage.BaseValue;
        var owner = card.Owner;
        if (owner?.Creature == null) return amount;

        try
        {
            // 引数順: target → dealer（CalculatedDamageVar と同じ）
            return Hook.ModifyDamage(
                owner.RunState,
                card.CombatState,
                target,
                owner.Creature,
                amount,
                ValueProp.Move,
                card,
                cardPlay: null,
                ModifyDamageHookType.All,
                CardPreviewMode.Normal,
                out _);
        }
        catch
        {
            return amount;
        }
    }

    private static string FormatDamage(decimal amount)
    {
        var culture = LocManager.Instance?.CultureInfo;
        return amount == decimal.Truncate(amount)
            ? ((int)amount).ToString(culture)
            : amount.ToString("0.##", culture);
    }
}
