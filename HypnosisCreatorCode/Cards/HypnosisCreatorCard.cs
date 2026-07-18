using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

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

    /// <summary>複数タグを種類ごとに刺す。未指定時はタグ2種以上なら自動で個別。</summary>
    public virtual bool? FetishHitPerTypeOverride => null;

    /// <summary>性癖が刺さりうるとき黄色ハイライト（mechanics-lock）。</summary>
    protected override bool ShouldGlowGoldInternal =>
        FetishGlow.ShouldGlow(this) || base.ShouldGlowGoldInternal;

    protected static IEnumerable<CardKeyword> CountKeywords =>
        [HcKeywords.Count, CardKeyword.Retain, CardKeyword.Exhaust];

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
