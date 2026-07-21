using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// ファブリケーターの心臓 — 希少。
/// CSV 効果欄が空のため、現状は発動で消費のみ（効果追記待ち）。
/// </summary>
public class FabricatorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FABRICATOR";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        // CSV No.187 効果未記載。未発動扱いだと No.86／自己暗示と相性が悪いため消費のみ。
        Flash();
        MarkUsed();
        return Task.CompletedTask;
    }
}
