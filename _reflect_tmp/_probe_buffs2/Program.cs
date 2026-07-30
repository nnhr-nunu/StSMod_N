using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var removeOnDeath = hook.GetMethod("ShouldPowerBeRemovedOnDeath", BindingFlags.Public | BindingFlags.Static)!;

bool IsBuff(Type t)
{
    var m = t.GetMethod("get_Type", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    if (m == null) return false;
    var il = m.GetMethodBody()?.GetILAsByteArray();
    return il is [0x17, 0x2A]; // ldc.i4.1 ret
}

void Check(string name)
{
    var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
    if (t == null) { Console.WriteLine($"{name}: NOT FOUND"); return; }
    var buff = IsBuff(t);
    // invoke ShouldPowerBeRemovedOnDeath needs instance - skip
    Console.WriteLine($"{name}: Buff={buff}");
}

foreach (var n in new[]
{
    "MinionPower", "ArtifactPower", "TerritorialPower", "SoarPower", "FlutterPower",
    "BufferPower", "CurlUpPower", "BurrowedPower", "GuardedPower", "PlatingPower",
    "IntangiblePower", "ConstrictPower", "AsleepPower", "IllusionPower", "SandpitPower",
    "SurprisePower", "SkittishPower", "ReattachPower", "SteamEruptionPower",
    "MalleablePower", "SharpPower", "VigorPower", "ThornsPower", "MetallicizePower"
})
    Check(n);
