using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// リグラー／寄生キャエルの心臓 — 希少。
/// 手札にプレイ可能な感染カード1枚を加える。
/// </summary>
public class WrigglerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "WRIGGLER";

    /// <summary>寄生キャエル（Phrog Parasite）も同一心臓。リーサルで入手可。</summary>
    public override IReadOnlyList<string> MonsterIdEntries =>
        ["WRIGGLER", "PHROG_PARASITE"];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<Infect>(false);

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState == null) return;

        Flash();
        await StatusHypnosisConvert.AddPlayableStatusAsync<Infect>(player, 1, PileType.Hand);
        MarkUsed();
    }
}
