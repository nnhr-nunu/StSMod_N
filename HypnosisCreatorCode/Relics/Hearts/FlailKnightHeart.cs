using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>フレイルナイトの心臓 — 希少。筋力3。</summary>
public class FlailKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FLAIL_KNIGHT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<StrengthPower>(
            this, choiceContext, player, 3);
}
