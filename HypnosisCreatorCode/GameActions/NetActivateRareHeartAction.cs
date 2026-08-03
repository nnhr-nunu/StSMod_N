using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace HypnosisCreator.HypnosisCreatorCode.GameActions;

/// <summary>マルチ同期用 — 希少心臓の右クリック発動。</summary>
public struct NetActivateRareHeartAction : INetAction, IPacketSerializable
{
    public int relicIndex;

    public GameAction ToGameAction(Player player) =>
        new ActivateRareHeartAction(player, relicIndex, enqueuedInCombat: true);

    public void Serialize(PacketWriter writer) => writer.WriteInt(relicIndex, 8);

    public void Deserialize(PacketReader reader) => relicIndex = reader.ReadInt(8);

    public override string ToString() => $"NetActivateRareHeartAction index: {relicIndex}";
}
