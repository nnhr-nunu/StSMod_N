using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;

foreach (var m in cm.GetMethods(BindingFlags.Public | BindingFlags.Instance))
    if (m.Name.Contains("Secondary") || m.Name.Contains("Primary"))
        Console.WriteLine($"CombatManager.{m.Name}");

foreach (var nested in cm.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
    {
        if (m.Name.Contains("98_0") || m.Name.Contains("99_0"))
        {
            var mil = m.GetMethodBody()!.GetILAsByteArray()!;
            Console.WriteLine($"\n{nested.Name}.{m.Name}:");
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

// Who calls get_IsSecondaryEnemy on Creature?
foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var mil = m.GetMethodBody()?.GetILAsByteArray();
        if (mil == null || mil.Length > 25) continue;
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    if (member?.Name == "get_IsSecondaryEnemy")
                        Console.WriteLine($"{t.Name}.{m.Name} -> {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}
