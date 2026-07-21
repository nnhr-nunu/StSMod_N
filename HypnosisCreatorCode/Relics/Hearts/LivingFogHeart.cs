using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>生きた霧の心臓 — 希少。爆発アンプル（CSV: bomb）を入手。</summary>
public class LivingFogHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LIVING_FOG";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRarePotion<ExplosiveAmpoule>(this, player);
}
