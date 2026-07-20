using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Cards;

[Pool(typeof(HypnosisCreatorCardPool))]
public abstract class HypnosisCreatorCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true, bool autoAdd = true) :
    CustomCardModel(cost, type, rarity, target, showInCardLibrary, autoAdd)
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public virtual IReadOnlyList<FetishType> CardFetishes => [];
    public virtual bool AlwaysHitsFetish => false;

    /// <summary>
    /// カード固有ホバー。トランス付与カードは自動でトランス説明を足すので、
    /// サブクラスはこちらを上書きする（ExtraHoverTips は使わない）。
    /// </summary>
    protected virtual IEnumerable<IHoverTip> CardHoverTips => [];

    /// <summary>CanonicalVars に Trance がある／トランス付与カード向け。</summary>
    protected virtual bool IncludesTranceHoverTip =>
        DynamicVars.Values.Any(v => v.Name == "Trance");

    protected sealed override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var tip in CardHoverTips)
                yield return tip;
            if (IncludesTranceHoverTip)
                yield return HoverTipFactory.FromPower<TrancePower>();
        }
    }

    /// <summary>複数タグを種類ごとに刺す。未指定時はタグ2種以上なら自動で個別。</summary>
    public virtual bool? FetishHitPerTypeOverride => null;

    /// <summary>
    /// 黄色ハイライト: 性癖が刺さる／条件達成（トランスあり・引き寄せ済み等）のとき。
    /// </summary>
    protected override bool ShouldGlowGoldInternal =>
        (FetishGlowAllowed && FetishGlow.ShouldGlow(this))
        || ShouldGlowWhenConditionMet()
        || base.ShouldGlowGoldInternal;

    /// <summary>
    /// false のとき性癖一致だけでは光らない（プレイ条件未達のカード用）。
    /// </summary>
    protected virtual bool FetishGlowAllowed => true;

    /// <summary>使用価値がある状況での黄ハイライト。各カードが条件を上書きする。</summary>
    protected virtual bool ShouldGlowWhenConditionMet() => false;

    /// <summary>照準中ならその対象、そうでなければ生存中の敵のいずれかで predicate が真なら光る。</summary>
    protected bool GlowIfTargetOrAnyEnemy(Func<Creature, bool> predicate)
    {
        var combat = CombatState;
        if (combat == null) return false;

        if (CurrentTarget != null)
            return CurrentTarget.IsAlive && predicate(CurrentTarget);

        return combat.HittableEnemies.Any(e => e.IsAlive && e.IsEnemy && predicate(e));
    }

    /// <summary>生存中の敵のいずれかで predicate が真なら光る（自分対象カード向け）。</summary>
    protected bool GlowIfAnyEnemy(Func<Creature, bool> predicate)
    {
        var combat = CombatState;
        if (combat == null) return false;
        return combat.HittableEnemies.Any(e => e.IsAlive && e.IsEnemy && predicate(e));
    }

    /// <summary>
    /// カウント共通キーワード。Retain は機能用（説明欄の「保留。」は CountCardText で抑制）。
    /// </summary>
    protected static IEnumerable<CardKeyword> CountKeywords =>
        [HcKeywords.Count, CardKeyword.Retain, CardKeyword.Exhaust];

    /// <summary>
    /// 本家パワーと同様、プレイ演出キューで PowerUp（連番では cast）を再生する。
    /// 個別 OnPlay だけでは TriggerAnim が呼ばれず、詠唱モーションが出ない。
    /// </summary>
    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        if (Type == CardType.Power && Owner?.Creature != null)
        {
            await CreatureCmd.TriggerAnim(
                Owner.Creature,
                "PowerUp",
                Owner.Character.PowerUpAnimDelay);
        }

        await base.OnEnqueuePlayVfx(target);
    }

    protected bool ShouldSingleFetishHit()
    {
        if (FetishHitPerTypeOverride == true) return false;
        if (FetishHitPerTypeOverride == false) return true;
        if (AlwaysHitsFetish) return true; // 感度3000倍: 必ず1回
        return CardFetishes.Count <= 1;
    }

    protected async Task<int> ResolveFetishOnTarget(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null) return 0;
        if (CardFetishes.Count == 0 && !AlwaysHitsFetish) return 0;
        return await FetishCombat.TryFetishHit(
            choiceContext, play.Target, Owner.Creature, this, CardFetishes, AlwaysHitsFetish,
            singleHit: ShouldSingleFetishHit());
    }

    protected async Task ResolveFetishOnAllEnemies(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;
        if (CardFetishes.Count == 0 && !AlwaysHitsFetish) return;
        var single = ShouldSingleFetishHit();
        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await FetishCombat.TryFetishHit(
                choiceContext, enemy, Owner.Creature, this, CardFetishes, AlwaysHitsFetish,
                singleHit: single);
        }
    }
}
