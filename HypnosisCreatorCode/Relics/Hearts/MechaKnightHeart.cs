using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// メカナイトの心臓 — 希少。
/// 相手へプレイ可能な火傷を4枚手札に加える。
/// </summary>
public class MechaKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "MECHA_KNIGHT";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<AbnormalBurn>(false);

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState == null) return;

        Flash();
        await StatusHypnosisConvert.AddFreePlayableAsync<AbnormalBurn>(player, 4, PileType.Hand);
        MarkUsed();
    }
}
