using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>死体ナメクジの心臓 — 希少。筋力+1。</summary>
public class CorpseSlugHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "CORPSE_SLUG";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<StrengthPower>(this, choiceContext, player, 1);
}
