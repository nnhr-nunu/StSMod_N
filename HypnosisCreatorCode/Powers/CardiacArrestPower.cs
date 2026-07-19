using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 心停止催眠 — 残りターン数を毎プレイヤーターン開始時に1減らし、0になると即死させる。
/// UG時は停止時に敵固有心臓を「追加のレリック報酬」として戦闘報酬へ載せる。
/// </summary>
public class CardiacArrestPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool GrantBonusRelic { get; set; }
    public Player? BonusRelicPlayer { get; set; }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        BonusRelicPlayer ??= applier?.Player ?? cardSource?.Owner;
        if (cardSource is CardiacArrestHypnosis { IsUpgraded: true })
            GrantBonusRelic = true;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;

        var target = Owner;
        await PowerCmd.Decrement(this);
        if (target.GetPower<CardiacArrestPower>() == null || target.GetPowerAmount<CardiacArrestPower>() <= 0)
        {
            if (GrantBonusRelic && BonusRelicPlayer != null)
            {
                // 死亡前に ID を確定し、通常報酬と並ぶ追加報酬スロットへ載せる
                HeartCapture.TryAddExtraRelicReward(BonusRelicPlayer, target);
            }

            await CreatureCmd.Kill(target);
        }
    }
}
