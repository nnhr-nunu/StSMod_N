using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// バトルフレンドの心臓 — 希少。デッキのランダムなカード1枚を永続アップグレード。
/// </summary>
public class BattleFriendHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "BATTLE_FRIEND_V1";

    public override IReadOnlyList<string> MonsterIdEntries =>
        ["BATTLE_FRIEND_V1", "BATTLE_FRIEND_V2", "BATTLE_FRIEND_V3"];

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var candidates = player.Deck.Cards.Where(c => c.IsUpgradable).ToList();
        if (candidates.Count == 0) return Task.CompletedTask;

        var rng = player.RunState.Rng.CombatCardSelection;
        var pick = candidates[rng.NextInt(candidates.Count)];

        Flash();
        CardCmd.Upgrade(pick, CardPreviewStyle.None);
        MarkUsed();
        return Task.CompletedTask;
    }
}
