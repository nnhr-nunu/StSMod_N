using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>グローブヘッドの心臓 — 希少。パワープレイ時6ブロックのバフ。</summary>
public class GlobeHeadHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "GLOBE_HEAD";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<GlobeHeadPower>(
            choiceContext, player.Creature, 6, player.Creature, null!);
        MarkUsed();
    }
}
