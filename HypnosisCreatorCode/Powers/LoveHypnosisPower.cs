using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 好き好き催眠 — 専用デバフ。バフ行動（UG時はブロックも）の着弾先をプレイヤーへ寄せる。
/// 実際の付け替えは <c>LoveHypnosisRedirectPatch</c> が担当する。
/// </summary>
public class LoveHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>バフ意図の PowerCmd.Apply をプレイヤーへ。</summary>
    public bool StealBuff { get; set; } = true;

    /// <summary>防御意図の GainBlock をプレイヤーへ（UG）。</summary>
    public bool StealBlock { get; set; }

    public override LocString Description
    {
        get
        {
            if (!StealBlock) return base.Description;
            var loc = new LocString(base.Description.LocTable, "HYPNOSISCREATOR-LOVE_HYPNOSIS_POWER.description_with_block");
            return loc;
        }
    }

    public Creature? ResolvePlayerCreature()
    {
        if (Applier is { IsPlayer: true }) return Applier;
        return Applier?.Player?.Creature;
    }

    public bool IsStealingBuffMove()
    {
        if (!StealBuff || Owner?.Monster is not { IsPerformingMove: true } monster) return false;
        return monster.NextMove?.Intents?.OfType<BuffIntent>().Any() == true;
    }

    public bool IsStealingBlockMove()
    {
        if (!StealBlock || Owner?.Monster is not { IsPerformingMove: true } monster) return false;
        return monster.NextMove?.Intents?.OfType<DefendIntent>().Any() == true;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 敵の行動ターンが終わったら解除（予定行動の奪取用）
        if (side != CombatSide.Enemy || Owner == null) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
