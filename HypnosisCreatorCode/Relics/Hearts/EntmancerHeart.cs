using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>エントマンサーの心臓 — 希少。ランダム敵に3ダメージ×7。</summary>
public class EntmancerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "ENTOMANCER";

    protected override decimal PreviewDamage => 3;
    protected override int PreviewHits => 7;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue, 7);
}
