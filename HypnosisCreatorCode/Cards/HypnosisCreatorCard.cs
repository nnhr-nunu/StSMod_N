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

/// <summary>
/// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
/// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public abstract class HypnosisCreatorCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    /// <summary>このカードが持つ性癖タグ。刺さり判定に使う。</summary>
    public virtual IReadOnlyList<FetishType> CardFetishes => [];

    /// <summary>スロット無視で性癖刺さりを発動する。</summary>
    public virtual bool AlwaysHitsFetish => false;

    /// <summary>壱佰捌煩悩など、刺さった種類ごとに破滅を別々に付与する。</summary>
    public virtual bool FetishHitPerType => false;

    /// <summary>カウント共通キーワード（保留・廃棄）。コスト0制約は HcKeywords.Count。</summary>
    protected static IEnumerable<CardKeyword> CountKeywords =>
        [HcKeywords.Count, CardKeyword.Retain, CardKeyword.Exhaust];

    protected async Task ResolveFetishOnTarget(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null) return;
        if (CardFetishes.Count == 0 && !AlwaysHitsFetish) return;

        if (FetishHitPerType)
        {
            await FetishCombat.TryFetishHitPerType(
                choiceContext, play.Target, Owner.Creature, this, CardFetishes, AlwaysHitsFetish);
        }
        else
        {
            await FetishCombat.TryFetishHit(
                choiceContext, play.Target, Owner.Creature, this, CardFetishes, AlwaysHitsFetish);
        }
    }

    protected async Task ResolveFetishOnAllEnemies(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;
        if (CardFetishes.Count == 0 && !AlwaysHitsFetish) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            if (FetishHitPerType)
            {
                await FetishCombat.TryFetishHitPerType(
                    choiceContext, enemy, Owner.Creature, this, CardFetishes, AlwaysHitsFetish);
            }
            else
            {
                await FetishCombat.TryFetishHit(
                    choiceContext, enemy, Owner.Creature, this, CardFetishes, AlwaysHitsFetish);
            }
        }
    }
}
