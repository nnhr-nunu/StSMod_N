using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

// Vanilla powers: Buff type + ShouldPowerBeRemovedOnDeath check
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var pm = a.GetType("MegaCrit.Sts2.Core.Models.PowerModel")!;

foreach (var t in a.GetTypes().Where(x => x.Namespace?.Contains("Models.Powers") == true && x.IsSubclassOf(pm)))
{
    var getType = t.GetMethod("get_Type", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    if (getType == null) continue;
    var il = getType.GetMethodBody()?.GetILAsByteArray();
    if (il == null || il.Length > 4) continue;
    // ldc.i4.1 = Buff
    if (il[0] != 0x17 || il[1] != 0x2A) continue;

    var name = t.Name;
    // skip common strippable
    if (name is "StrengthPower" or "RitualPower" or "MetallicizePower" or "PlatedArmorPower") continue;

    Console.WriteLine(name);
}
