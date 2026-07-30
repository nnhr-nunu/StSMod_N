using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes())
{
    foreach (var mt in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (!mt.Name.EndsWith("99_0")) continue;
        var mil = mt.GetMethodBody()!.GetILAsByteArray()!;
        Console.WriteLine($"{t.Name}.{mt.Name}:");
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = mt.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

var c = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
var m = c.GetMethod("get_IsPrimaryEnemy", BindingFlags.Public | BindingFlags.Instance)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nCreature.IsPrimaryEnemy:");
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
