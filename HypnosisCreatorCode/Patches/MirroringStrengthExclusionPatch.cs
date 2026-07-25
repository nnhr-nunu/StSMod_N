using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ミラーリングは Move でスロウ等を載せつつ、CSVどおりプレイヤー筋力だけ乗せない。
/// </summary>
[HarmonyPatch(typeof(StrengthPower), nameof(StrengthPower.ModifyDamageAdditive))]
public static class MirroringStrengthExclusionPatch
{
    public static bool Prefix(
        CardModel cardSource,
        decimal amount,
        ref decimal __result)
    {
        if (cardSource is not Mirroring) return true;

        __result = amount;
        return false;
    }
}
