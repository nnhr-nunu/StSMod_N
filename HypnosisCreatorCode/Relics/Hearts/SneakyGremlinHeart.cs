using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>忍びグレmlinの心臓 — 希少。ランダム敵に9ダメージ。</summary>
public class SneakyGremlinHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SNEAKY_GREMLIN";

    protected override decimal PreviewDamage => 9;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue);
}
