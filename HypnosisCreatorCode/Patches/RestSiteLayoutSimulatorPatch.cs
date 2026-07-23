using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

[HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
public static class RestSiteLayoutSimulatorRoomReadyPatch
{
    public static void Postfix() =>
        Callable.From(RestSiteLayoutSimulator.Refresh).CallDeferred();
}

[HarmonyPatch(typeof(NRestSiteRoom), "BeforeExitingRoom")]
public static class RestSiteLayoutSimulatorRoomExitPatch
{
    public static void Prefix() => RestSiteLayoutSimulator.Clear();
}
