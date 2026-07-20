using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ロケットの心臓（カイザークラブ右爪）— 希少。最大HP+10。クラッシャーと同効果。</summary>
public class RocketHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "ROCKET";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 10);
}
