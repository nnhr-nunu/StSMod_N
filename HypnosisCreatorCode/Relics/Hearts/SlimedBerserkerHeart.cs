using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// スライムバーサーカーの心臓 — 希少。
/// 相手へプレイ可能な粘液を5枚捨て札に加える。
/// </summary>
public class SlimedBerserkerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SLIMED_BERSERKER";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<AbnormalSlime>(false);

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState == null) return;

        Flash();
        await StatusHypnosisConvert.AddFreePlayableAsync<AbnormalSlime>(player, 5, PileType.Discard);
        MarkUsed();
    }
}
