using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 敵固有心臓の基底。
/// 希少な心臓: 使用済みになると IsUsedUp。No.86 で再使用可能になる。
/// 非希少（最大HP・ゴールド等）: 入手時に効果。
/// </summary>
public abstract class EnemyHeartRelic : HypnosisCreatorRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    /// <summary>希少な心臓（右クリック／手動発動・1回限り）。</summary>
    public abstract bool IsRareHeart { get; }

    /// <summary>対応モンスター ID（抽選除外・捕獲マッピング用）。</summary>
    public abstract string MonsterIdEntry { get; }

    public bool WasUsed { get; set; }

    /// <summary>No.86 UG後は戦闘外でも再使用可。</summary>
    public bool PermanentlyReusable { get; set; }

    public override bool IsUsedUp => IsRareHeart && WasUsed && !PermanentlyReusable && !CombatReuseActive;

    /// <summary>No.86 戦闘中再使用フラグ。</summary>
    public bool CombatReuseActive { get; set; }

    public void RefreshForCombat()
    {
        if (!IsRareHeart) return;
        WasUsed = false;
        CombatReuseActive = true;
        Flash();
    }

    public void RefreshPermanently()
    {
        if (!IsRareHeart) return;
        WasUsed = false;
        PermanentlyReusable = true;
        CombatReuseActive = true;
        Flash();
    }

    public void MarkUsed()
    {
        if (!IsRareHeart) return;
        if (PermanentlyReusable) return;
        WasUsed = true;
        CombatReuseActive = false;
    }

    /// <summary>希少な心臓の発動本体。呼び出し側（UI／仮の戦闘開始トリガー）から使う。</summary>
    public abstract Task ActivateAsync(PlayerChoiceContext choiceContext, Player player);

    public override async Task AfterObtained()
    {
        await base.AfterObtained();
        if (!IsRareHeart)
            await OnPassiveObtain();
    }

    /// <summary>非希少心臓の入手時効果。</summary>
    protected virtual Task OnPassiveObtain() => Task.CompletedTask;

    /// <summary>
    /// No.85 で得た心臓は「戦闘開始トリガーを満たす」＝ AfterPlayerTurnStart turn1 で Activate。
    /// 手動発動はレリック枠の右クリック（<c>RareHeartRightClickPatch</c>）。
    /// </summary>
    public bool FireCombatStartTrigger { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (!FireCombatStartTrigger) return;
        if (Owner.PlayerCombatState?.TurnNumber != 1) return;
        if (!IsRareHeart || IsUsedUp) return;

        FireCombatStartTrigger = false;
        await ActivateAsync(choiceContext, player);
        // MarkUsed は ActivateAsync / Helper 側で効果成功時のみ行う。
        // ここでの二重 MarkUsed は、敵不在などで未発動の心臓を消費してしまう。
    }
}
