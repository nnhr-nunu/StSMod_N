using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var modelDb = a.GetType("MegaCrit.Sts2.Core.Models.ModelDb")!;
var allPowers = modelDb.GetProperty("AllPowers", BindingFlags.Public | BindingFlags.Static)!.GetValue(null) as System.Collections.IEnumerable;
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var shouldRemove = hook.GetMethod("ShouldPowerBeRemovedOnDeath", BindingFlags.Public | BindingFlags.Static)!;

foreach (var p in allPowers!.Cast<object>())
{
    var type = p.GetType();
    if (!type.Name.EndsWith("Power")) continue;
    if (!new[] { "MinionPower", "ArtifactPower", "TerritorialPower", "SoarPower", "FlutterPower",
        "BufferPower", "CurlUpPower", "BurrowedPower", "GuardedPower", "PlatingPower",
        "IntangiblePower", "VigorPower", "ThornsPower", "HardToKillPower", "HardenedShellPower" }.Contains(type.Name))
        continue;
    var getType = type.GetMethod("get_Type", BindingFlags.Public | BindingFlags.Instance)!;
    var pt = getType.Invoke(p, null);
    var removable = (bool)shouldRemove.Invoke(null, [p])!;
    Console.WriteLine($"{type.Name} Type={pt} RemoveOnDeath={removable}");
}
