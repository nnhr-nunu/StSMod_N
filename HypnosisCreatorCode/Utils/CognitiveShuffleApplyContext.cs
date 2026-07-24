using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル付与直前のキャラアイコン。BeforeApplied で消費する（NPower 生成前にパスを確定）。
/// </summary>
public static class CognitiveShuffleApplyContext
{
    private static readonly ConditionalWeakTable<Player, CharacterModel> Icons = new();

    public static void SetIconCharacter(Player? player, CharacterModel? character)
    {
        if (player == null || character == null) return;
        Icons.Remove(player);
        Icons.Add(player, character);
    }

    public static CharacterModel? TakeIconCharacter(Player? player)
    {
        if (player == null) return null;
        if (!Icons.TryGetValue(player, out var character)) return null;
        Icons.Remove(player);
        return character;
    }
}
