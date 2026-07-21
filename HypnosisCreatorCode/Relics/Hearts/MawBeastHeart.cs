using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>貪りの獣の心臓 — 希少。ぬぬ地獄（引き寄せ系＋15ダメージ）。</summary>
public class MawBeastHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THE_INSATIABLE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<NunuHellPower>(
            choiceContext, player.Creature, 15, player.Creature, null!);
        MarkUsed();
    }
}
