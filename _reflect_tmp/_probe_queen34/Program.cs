using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;

foreach (var t in new[] { cm, cs })
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
    {
        if (!m.Name.Contains("Secondary") && !m.Name.Contains("IsEnding") && !m.Name.Contains("Primary")) continue;
        Console.WriteLine($"{t.Name}.{m.Name}");
    }
}

// b__98_0 and b__99_0 - search all nested in CombatManager and CombatState
foreach (var t in new Type[] { cm, cs })
{
    foreach (var nested in t.GetNestedTypes(BindingFlags.NonPublic))
    {
        foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        {
            if (m.Name.Contains("98_0") || m.Name.Contains("99_0"))
            {
                var mil = m.GetMethodBody()!.GetILAsByteArray()!;
                Console.WriteLine($"\n{t.Name}.{nested.Name}.{m.Name}:");
                for (var i = 0; i < mil.Length; i++)
                {
                    if (mil[i] is 0x28 or 0x6F)
                    {
                        try
                        {
                            var member = m.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                        }
                        catch { }
                        i += 4;
                    }
                }
            }
        }
    }
}

// CombatState get_IsSecondaryEnemy
var ise = cs.GetMethod("get_IsSecondaryEnemy", BindingFlags.Public | BindingFlags.Instance)!;
var il = ise.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nCombatState.get_IsSecondaryEnemy:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = ise.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
