using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>マウビーストの心臓 — 希少。引き寄せ追加ダメージ+15。</summary>
public class MawBeastHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THE_INSATIABLE";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        PullTracker.ExtraDamage = 15;
        MarkUsed();
        return Task.CompletedTask;
    }
}
