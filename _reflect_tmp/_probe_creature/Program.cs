using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var c = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
var m = c.GetProperty("IsPlayer", BindingFlags.Public | BindingFlags.Instance)!.GetGetMethod(true)!;
var il = m.GetMethodBody()!.GetILAsByteArray();
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
        Console.WriteLine($"{member!.DeclaringType?.Name}.{member.Name}");
        i += 4;
    }
}
