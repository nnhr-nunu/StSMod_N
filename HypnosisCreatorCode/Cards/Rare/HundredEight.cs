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
/// 壱佰捌煩悩 — 全性癖タグ。敵全体の性癖を目覚めさせ、プレイ後コスト+1。
/// 3コストでプレイ時、すべての敵に1ダメージ×108回を与えて廃棄。
/// UG: プレイ後は山札に入る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HundredEight() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    private const int FinalCostThreshold = 3;
    private const int FinalHitCount = 108;
    private const string JpnUpgradeLine = "[green]プレイ後は山札に入る。[/green]";
    private const string EngUpgradeLine = "[green]After play, shuffle this into your draw pile.[/green]";

    static HundredEight()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendUpgradeLine;
    }

    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Sm, FetishType.DomSub, FetishType.Abnormal];

    public override bool AlwaysHitsFetish => true;
    public override bool? FetishHitPerTypeOverride => true;

    private static void AppendUpgradeLine(CardModel card, Creature? target, ref string description)
    {
        if (card is not HundredEight { IsUpgraded: true }) return;

        var line = IsJapaneseUi() ? JpnUpgradeLine : EngUpgradeLine;
        if (description.Contains(line, StringComparison.Ordinal)) return;
        description = description.TrimEnd() + "\n" + line;
    }

    private static bool IsJapaneseUi()
    {
        try
        {
            var lang = LocManager.Instance?.Language ?? "";
            return lang.Contains("jpn", StringComparison.OrdinalIgnoreCase)
                   || lang.Contains("ja", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>必中タグで常時光らないよう、コスト3到達（108連撃）のときだけ黄ハイライト。</summary>
    protected override bool ShouldGlowGoldInternal =>
        EnergyCost.GetResolved() >= FinalCostThreshold;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
            FetishCombat.AwakenAll(enemy, Owner);

        var resolvedCost = EnergyCost.GetResolved();
        if (resolvedCost >= FinalCostThreshold)
        {
            // 1ダメージ×108回を全敵へ。攻撃中は指パッチンをループ
            CombatFrameAnimator.BeginAttackLoop(Owner.Creature);
            try
            {
                foreach (var enemy in CombatState.HittableEnemies.ToList())
                {
                    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                        .WithHitCount(FinalHitCount)
                        .FromCard(this, play)
                        .Targeting(enemy)
                        .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
                        .WithNoAttackerAnim()
                        .Execute(choiceContext);
                }
            }
            finally
            {
                CombatFrameAnimator.EndAttackLoop(Owner.Creature);
            }
        }

        await ResolveFetishOnAllEnemies(choiceContext, play);

        if (resolvedCost < FinalCostThreshold)
            EnergyCost.AddThisCombat(1);
    }

    /// <summary>
    /// コスト3に達した最終カットではプレイ後に廃棄する。
    /// コスト判定は OnPlay より前に確定させる必要があるため、こちらで上書きする。
    /// </summary>
    protected override CardLocation GetResultLocationForCardPlay()
    {
        if (IsUpgraded)
            return new CardLocation(Owner, PileType.Draw, CardPilePosition.Random);

        return EnergyCost.GetResolved() >= FinalCostThreshold
            ? new CardLocation(Owner, PileType.Exhaust, CardPilePosition.Bottom)
            : base.GetResultLocationForCardPlay();
    }
}
