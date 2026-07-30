using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;

var harmony = new Harmony("test");
var target = AccessTools.Method(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount));
Console.WriteLine("Target: " + (target != null ? target.ToString() : "NULL"));

var prefix = typeof(P).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public)!;
try
{
    harmony.Patch(target, new HarmonyMethod(prefix));
    Console.WriteLine("Patch OK");
}
catch (Exception ex)
{
    Console.WriteLine("FAIL: " + ex);
}

static class P
{
    public static bool Prefix() => true;
}
