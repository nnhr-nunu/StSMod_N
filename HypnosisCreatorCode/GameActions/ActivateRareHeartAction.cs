using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.GameActions;

/// <summary>希少心臓の右クリック発動。全クライアントで同一キュー経由。</summary>
public sealed class ActivateRareHeartAction : GameAction
{
    private readonly Player _player;
    private readonly int _relicIndex;
    private readonly bool _enqueuedInCombat;

    public override ulong OwnerId => _player.NetId;

    public override GameActionType ActionType =>
        _enqueuedInCombat ? GameActionType.CombatPlayPhaseOnly : GameActionType.NonCombat;

    public PlayerChoiceContext? PlayerChoiceContext { get; private set; }

    public ActivateRareHeartAction(EnemyHeartRelic heart)
    {
        if (heart.Owner == null)
            throw new InvalidOperationException($"Cannot enqueue heart activation for {heart.Id.Entry} without owner.");

        _player = heart.Owner;
        _relicIndex = ResolveRelicIndex(heart, _player);
        _enqueuedInCombat = CombatManager.Instance?.IsInProgress == true;
    }

    public ActivateRareHeartAction(Player player, int relicIndex, bool enqueuedInCombat)
    {
        _player = player;
        _relicIndex = relicIndex;
        _enqueuedInCombat = enqueuedInCombat;
    }

    protected override async Task ExecuteAction()
    {
        if (_relicIndex < 0 || _relicIndex >= _player.Relics.Count)
        {
            MainFile.Logger.Warn($"ActivateRareHeartAction: invalid relic index {_relicIndex} for player {_player.NetId}");
            Cancel();
            return;
        }

        if (_player.Relics[_relicIndex] is not EnemyHeartRelic heart)
        {
            MainFile.Logger.Warn($"ActivateRareHeartAction: relic at {_relicIndex} is not EnemyHeartRelic");
            Cancel();
            return;
        }

        heart = HeartRelicActivation.ResolveOwnedHeart(heart, _player) ?? heart;
        if (!HeartRelicActivation.CanExecuteAction(heart, _player))
        {
            Cancel();
            return;
        }

        PlayerChoiceContext = new GameActionPlayerChoiceContext(this);
        await heart.ActivateAsync(PlayerChoiceContext, _player);
    }

    public override INetAction ToNetAction()
    {
        var net = default(NetActivateRareHeartAction);
        net.relicIndex = _relicIndex;
        return net;
    }

    public override string ToString() =>
        $"ActivateRareHeartAction player={_player.NetId} index={_relicIndex} combat={_enqueuedInCombat}";

    private static int ResolveRelicIndex(EnemyHeartRelic heart, Player player)
    {
        for (var i = 0; i < player.Relics.Count; i++)
        {
            var relic = player.Relics[i];
            if (ReferenceEquals(relic, heart) || relic.Id.Entry == heart.Id.Entry)
                return i;
        }

        throw new InvalidOperationException($"Heart {heart.Id.Entry} not found in player relic list.");
    }
}
