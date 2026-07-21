using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>噛みつきの巻物の心臓 — 希少。非ブロックダメージ時に追加2ダメージのバフ。</summary>
public class ScrollOfBitingHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SCROLL_OF_BITING";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<ScrollOfBitingPower>(
            choiceContext, player.Creature, 2, player.Creature, null!);
        MarkUsed();
    }
}
