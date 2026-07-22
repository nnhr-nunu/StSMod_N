using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心臓えぐり出し — 攻撃・アブノーマル・ハート。コスト1。15ダメージ。廃棄。
/// 未UG: リーサルで追加レリック報酬。
/// UG: 対象の破滅が残りHPの50%以上ならとどめ（通常戦闘のみ）＋追加レリック報酬。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HeartGouge() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(15M, ValueProp.Move)];

    protected override bool ShouldGlowWhenConditionMet() =>
        IsUpgraded && GlowIfTargetOrAnyEnemy(CanDoomExecute);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        if (IsUpgraded)
        {
            // UG: ダメージ後も生存しており、破滅が残りHPの50%以上ならとどめ（通常戦闘のみ）
            if (play.Target.IsAlive && CanDoomExecute(play.Target))
            {
                HeartCapture.TryAddExtraRelicReward(Owner, play.Target);
                await CreatureCmd.Kill(play.Target);
            }
        }
        else if (play.Target is { IsAlive: false })
        {
            // 未UG: リーサル時は報酬画面の追加レリックへ（解剖・心停止＋と同じ）
            HeartCapture.TryAddExtraRelicReward(Owner, play.Target);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    /// <summary>定性UGのみ（ダメージは15のまま）。</summary>
    protected override void OnUpgrade() { }

    /// <summary>
    /// とどめ条件: 通常戦闘（Monster）かつ 破滅×2 ≥ 残りHP。
    /// エリート／ボスでは発生しない。
    /// </summary>
    internal static bool CanDoomExecute(Creature target)
    {
        if (!target.IsAlive) return false;
        if (!IsNormalCombat(target)) return false;

        var doom = target.GetPowerAmount<DoomPower>();
        if (doom <= 0) return false;

        // 破滅が残りHPの50%以上 ⇔ doom * 2 >= remainingHp
        return doom * 2 >= target.CurrentHp;
    }

    private static bool IsNormalCombat(Creature target) =>
        target.CombatState?.Encounter?.RoomType == RoomType.Monster;
}
