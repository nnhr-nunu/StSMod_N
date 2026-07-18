using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>スライム催眠中は敵名を抽選したスライム名へ差し替える。</summary>
[HarmonyPatch(typeof(Creature), "get_Name")]
public static class SlimeDisguiseNamePatch
{
    public static void Postfix(Creature __instance, ref string __result)
    {
        var power = __instance.GetPower<SlimeHypnosisPower>();
        if (power?.DisguiseName is { Length: > 0 } name)
            __result = name;
    }
}
