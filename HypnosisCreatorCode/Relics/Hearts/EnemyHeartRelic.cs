using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 敵固有心臓の基底。
/// CSV No.111〜はすべて希少な心臓（右クリック発動・各層1回）。
/// 使用済みは IsUsedUp。層クリアで再使用可。No.86 で戦闘中／永続再使用。
/// </summary>
public abstract class EnemyHeartRelic : HypnosisCreatorRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    /// <summary>希少な心臓（右クリック／手動発動・各層1回）。常に true。</summary>
    public virtual bool IsRareHeart => true;

    /// <summary>対応モンスター Id.Entry の代表値（ローカライズ／アイコン生成用）。</summary>
    public abstract string MonsterIdEntry { get; }

    /// <summary>
    /// 捕獲・抽選除外に使う実ゲーム Id.Entry 一覧。
    /// カイザークラブ左右爪・ムカデ節など、複数 ID が同一心臓に対応する場合に上書きする。
    /// </summary>
    public virtual IReadOnlyList<string> MonsterIdEntries => [MonsterIdEntry];

    /// <summary>プレビュー用ダメージ（0で無し）。筋力反映対象。</summary>
    protected virtual decimal PreviewDamage => 0;

    /// <summary>プレビュー用ヒット数（多段）。</summary>
    protected virtual int PreviewHits => 1;

    /// <summary>プレビュー用ブロック基礎値（0で無し）。敏捷反映対象。</summary>
    protected virtual decimal PreviewBlock => 0;

    /// <summary>true ならブロック＝所持心臓数×PreviewBlock（オビコプター）。</summary>
    protected virtual bool PreviewBlockPerOwnedHeart => false;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            if (PreviewDamage > 0)
            {
                yield return new DamageVar(PreviewDamage, ValueProp.Move);
                if (PreviewHits > 1)
                    yield return new DynamicVar("Hits", PreviewHits);
            }

            if (PreviewBlock > 0 || PreviewBlockPerOwnedHeart)
                yield return new BlockVar(PreviewBlock > 0 ? PreviewBlock : 2m, ValueProp.Move);

            if (PreviewBlockPerOwnedHeart)
                yield return new DynamicVar("Hearts", 1m);
        }
    }

    public bool WasUsed { get; set; }

    /// <summary>No.86 UG後は戦闘外でも再使用可。</summary>
    public bool PermanentlyReusable { get; set; }

    public override bool IsUsedUp => IsRareHeart && WasUsed && !PermanentlyReusable && !CombatReuseActive;

    /// <summary>No.86 戦闘中再使用フラグ。</summary>
    public bool CombatReuseActive { get; set; }

    /// <summary>
    /// No.86 未UG: 使用済み心臓だけ戦闘中再使用可。
    /// WasUsed は維持し、戦闘終了で <see cref="EndCombatReuse"/> すれば再び使用済みに戻る。
    /// 未使用の心臓は触らない。
    /// </summary>
    public void RefreshForCombat()
    {
        if (!IsRareHeart) return;
        if (PermanentlyReusable) return;
        if (!WasUsed) return;
        CombatReuseActive = true;
        Flash();
    }

    public void RefreshPermanently()
    {
        if (!IsRareHeart) return;
        WasUsed = false;
        PermanentlyReusable = true;
        CombatReuseActive = false;
        Flash();
    }

    /// <summary>戦闘終了時: 戦闘中再使用フラグを落とし、使用済み状態を復元する。</summary>
    public void EndCombatReuse()
    {
        CombatReuseActive = false;
    }

    /// <summary>次の層へ進んだとき、当該層での使用済みフラグをリセットする。</summary>
    public void RefreshForNewAct()
    {
        if (!IsRareHeart) return;
        if (PermanentlyReusable) return;

        WasUsed = false;
        CombatReuseActive = false;
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
    /// 手動発動はレリック枠の右クリック（<c>HeartRelicInputPatch</c>）。
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
