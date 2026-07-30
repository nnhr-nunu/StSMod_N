using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes())
{
    foreach (var mt in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (!mt.Name.Contains("HittableEnemies") && mt.Name != "get_IsEnding") continue;
        var mil = mt.GetMethodBody()?.GetILAsByteArray();
        if (mil == null) continue;
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = mt.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    if (member?.Name.Contains("Hittable") == true || member?.Name.Contains("StopCombat") == true)
                        Console.WriteLine($"{t.Name}.{mt.Name} -> {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

// Decompile get_IsEnding
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
var m = cm.GetMethod("get_IsEnding", BindingFlags.Public | BindingFlags.Instance)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nCombatManager.get_IsEnding:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
