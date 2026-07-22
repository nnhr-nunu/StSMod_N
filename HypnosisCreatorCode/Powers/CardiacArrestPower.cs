using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 心停止 — 残りターンを数え、0になった後の対象の行動終了時に即死する。
/// 重ねがけ（心停止催眠の再適用）は <see cref="AdvanceCountdown"/> で残りを1減らす。
/// </summary>
public class CardiacArrestPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool GrantBonusRelic { get; set; }
    public Player? BonusRelicPlayer { get; set; }

    /// <summary>
    /// 付与時点の敵 ID（キル時に Monster 参照が消えて StolenHeart 落ちするのを防ぐ）。
    /// </summary>
    public string? CapturedMonsterId { get; set; }

    /// <summary>
    /// 残り0になったあと、対象の行動終了時に即死させる。
    /// このあいだ Amount=0 でもパワーを残す（<c>CardiacArrestKeepAtZeroPatch</c>）。
    /// </summary>
    public bool KillAfterActionEnd { get; set; }

    /// <summary>心停止＋の追加心臓を既に予約済みか（複数体同時到達・撃破フック欠落の二重付与防止）。</summary>
    private bool _bonusHeartGranted;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        BonusRelicPlayer ??= applier?.Player ?? cardSource?.Owner;
        if (cardSource is CardiacArrestHypnosis { IsUpgraded: true })
            GrantBonusRelic = true;

        CaptureMonsterId();
        return Task.CompletedTask;
    }

    /// <summary>重ねがけ: 残りターンを1進め（減らし）、0なら行動終了時キル待ちにする。</summary>
    public static async Task AdvanceCountdown(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel cardSource)
    {
        var power = target.GetPower<CardiacArrestPower>();
        if (power == null) return;

        if (cardSource is CardiacArrestHypnosis { IsUpgraded: true })
            power.GrantBonusRelic = true;
        power.BonusRelicPlayer ??= applier.Player ?? cardSource.Owner;
        power.CaptureMonsterId();

        // すでに残り0（行動終了待ち）なら、重ねがけで即心停止する。
        if (power.KillAfterActionEnd || power.Amount <= 0)
        {
            await power.ExecuteKill();
            return;
        }

        await power.TickCountdown();
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (KillAfterActionEnd) return;

        await TickCountdown();
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (!participants.Contains(Owner)) return;
        if (!KillAfterActionEnd) return;

        await ExecuteKill();
    }

    private void CaptureMonsterId()
    {
        if (Owner == null) return;
        var id = HeartRegistry.GetMonsterId(Owner);
        if (!string.IsNullOrWhiteSpace(id))
            CapturedMonsterId = id;
    }

    private async Task TickCountdown()
    {
        if (KillAfterActionEnd) return;

        if (Amount <= 1)
        {
            KillAfterActionEnd = true;
            SetAmount(0, true);
            GrantBonusHeartIfNeeded();
            return;
        }

        await PowerCmd.Decrement(this);
    }

    public async Task ExecuteKill()
    {
        if (Owner == null || !Owner.IsAlive) return;

        GrantBonusHeartIfNeeded();

        await CreatureCmd.Kill(Owner);
    }

    /// <summary>
    /// 心停止＋の追加心臓を戦闘報酬へ載せる。
    /// 残り0到達時にも呼ぶ（同一ターン複数体・撃破フック欠落でも各体ぶん付与する）。
    /// </summary>
    private void GrantBonusHeartIfNeeded()
    {
        if (_bonusHeartGranted || !GrantBonusRelic || BonusRelicPlayer == null || Owner == null)
            return;

        CaptureMonsterId();
        var monsterId = CapturedMonsterId ?? HeartRegistry.GetMonsterId(Owner);
        if (!string.IsNullOrWhiteSpace(monsterId))
        {
            MainFile.Logger.Info(
                $"CardiacArrest heart from stored id={monsterId} (owner={Owner.Monster?.Id.Entry}/{Owner.ModelId.Entry})");
            // 文字列経由＝同心臓兄弟ゲートなし（ムカデ節ごと／カイザー左右）
            HeartCapture.TryAddExtraRelicReward(BonusRelicPlayer, monsterId, Owner);
        }
        else
        {
            MainFile.Logger.Warn(
                $"CardiacArrest heart: no monster id; owner ModelId={Owner.ModelId.Entry}");
            HeartCapture.TryAddExtraRelicReward(
                BonusRelicPlayer, Owner, allowWhileSiblingsAlive: true);
        }

        _bonusHeartGranted = true;
    }
}
