using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>マウビーストの心臓 — 希少。引き寄せ追加ダメージ+15。</summary>
public class MawBeastHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "MAW_BEAST";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        PullTracker.ExtraDamage = 15;
        MarkUsed();
        return Task.CompletedTask;
    }
}
