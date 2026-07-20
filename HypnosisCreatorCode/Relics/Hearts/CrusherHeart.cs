using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>クラッシャーの心臓（カイザークラブ左爪）— 希少。最大HP+10。ロケットと同効果。</summary>
public class CrusherHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "CRUSHER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 10);
}
