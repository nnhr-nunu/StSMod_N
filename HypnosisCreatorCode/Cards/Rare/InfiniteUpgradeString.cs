using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 糸色丁頁 — カウント。対象はHP19を失う。トランス1。
/// 無限UG：1回目でリプレイ1、以降UGするたびリプレイ+1（説明にリプレイは書かない）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteUpgradeString() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override int MaxUpgradeLevel => int.MaxValue;
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("LoseHp", 19M),
        new DynamicVar("Trance", 1M),
        new DynamicVar("Replays", 0M)
    ];

    /// <summary>GeneratePlayCount Prefix から呼ぶ。PlayCount 確定より前に BaseReplayCount をセットする。</summary>
    internal void PrepareReplay()
    {
        BaseReplayCount = Math.Max(0, DynamicVars["Replays"].IntValue);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars["LoseHp"].BaseValue, ValueProp.Move, Owner.Creature, this, play);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Replays"].UpgradeValueBy(1M);
        SyncReplayCount();
    }

    private void SyncReplayCount() =>
        BaseReplayCount = Math.Max(0, DynamicVars["Replays"].IntValue);
}
