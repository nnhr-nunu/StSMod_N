using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>知識の悪魔の心臓 — 希少。HP15回復。</summary>
public class KnowledgeDemonHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "KNOWLEDGE_DEMON";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfHeal(this, choiceContext, player, 15);
}
