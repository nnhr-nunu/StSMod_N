using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 永劫の砂時計の心臓 — 希少。カード6枚ごとにプレイ可能な衰微を手札へ。
/// </summary>
public class AeonglassHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "AEONGLASS";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<AeonglassPower>(
            choiceContext, player.Creature, 1, player.Creature, null!);
        MarkUsed();
    }
}
