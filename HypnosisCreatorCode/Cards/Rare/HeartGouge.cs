using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心臓えぐり出し — 攻撃・アブノーマル・ハート。コスト1。
/// 弱体1→15ダメージ。リーサルで追加レリック報酬。廃棄。
/// UG: 生存かつ破滅≥残りHP50%で破滅とどめ（通常戦闘のみ）＋同報酬。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartGouge() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1M),
        new DamageVar(15M, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override bool ShouldGlowWhenConditionMet() =>
        IsUpgraded && GlowIfTargetOrAnyEnemy(CanDoomExecute);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            play.Target,
            DynamicVars["VulnerablePower"].BaseValue,
            Owner.Creature,
            this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        if (play.Target is { IsAlive: false })
        {
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);
        }
        else if (IsUpgraded && CanDoomExecute(play.Target))
        {
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);
            await CreatureCmd.Kill(play.Target);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    /// <summary>定性UGのみ（ダメージ・弱体は据え置き）。</summary>
    protected override void OnUpgrade() { }

    internal static decimal ComputeDamagePreview(
        HeartGouge card,
        Creature? target,
        CardPreviewMode previewMode,
        bool runGlobalHooks = true)
    {
        var raw = card.DynamicVars.Damage.BaseValue;
        // 神秘のライター（攻撃+9）等は Hook.ModifyDamage 内で基礎値に加算される。先に足すと二重計上になる。
        return CardDamagePreview.ApplyAfterSelfVulnerable(
            card,
            target ?? card.CurrentTarget,
            raw,
            card.DynamicVars["VulnerablePower"].BaseValue,
            ValueProp.Move,
            previewMode,
            runGlobalHooks);
    }

    /// <summary>とどめ条件: 通常戦闘かつ 破滅×2 ≥ 残りHP。</summary>
    internal static bool CanDoomExecute(Creature target)
    {
        if (!target.IsAlive) return false;
        if (!IsNormalCombat(target)) return false;

        var doom = target.GetPowerAmount<DoomPower>();
        if (doom <= 0) return false;

        return doom * 2 >= target.CurrentHp;
    }

    private static bool IsNormalCombat(Creature target) =>
        target.CombatState?.Encounter?.RoomType == RoomType.Monster;
}
