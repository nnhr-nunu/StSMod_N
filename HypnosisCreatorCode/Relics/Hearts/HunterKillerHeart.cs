using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ハンターキラーの心臓 — 希少。ランダム敵に7ダメージ×3。</summary>
public class HunterKillerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "HUNTER_KILLER";

    protected override decimal PreviewDamage => 7;
    protected override int PreviewHits => 3;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, DynamicVars.Damage.BaseValue, 3);
}
