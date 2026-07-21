using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ヒキジャクシの心臓 — 希少。トゲ+2。</summary>
public class ToadKingHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TOADPOLE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<ThornsPower>(this, choiceContext, player, 2);
}
