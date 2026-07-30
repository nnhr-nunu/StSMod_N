using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;

foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
    {
        var il = m.GetMethodBody()?.GetILAsByteArray();
        if (il == null) continue;
        bool hasMinion = false;
        bool hasAlive = false;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    if (member?.Name.Contains("Minion") == true) hasMinion = true;
                    if (member?.Name.Contains("IsAlive") == true || member?.Name.Contains("Hittable") == true) hasAlive = true;
                }
                catch { }
                i += 4;
            }
        }
        if (hasMinion || (hasAlive && m.Name.Contains("b__")))
            Console.WriteLine($"{nested.Name}.{m.Name} minion={hasMinion} alive/hittable={hasAlive}");
    }
}

// AdaptablePower full - Respawns, ShouldDisappearFromDoom
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
foreach (var p in ap.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"AdaptablePower.{p.Name}");
