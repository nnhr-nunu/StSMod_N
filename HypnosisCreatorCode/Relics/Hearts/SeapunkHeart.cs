using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>シーパンクの心臓 — 希少。ランダム敵に2ダメージ×4。</summary>
public class SeapunkHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SEAPUNK";

    protected override decimal PreviewDamage => 2;
    protected override int PreviewHits => 4;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue, 4);
}
