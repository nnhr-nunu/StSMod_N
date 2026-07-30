using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;

foreach (var name in new[] { "get_IsSingleTargeted", "get_IsMultiTargeted" })
{
    var m = ac.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).First(x => x.Name == name);
    var il = m.GetMethodBody()!.GetILAsByteArray();
    Console.WriteLine($"{name}:");
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
            i += 4;
        }
        else
            Console.Write($"{il[i]:X2} ");
    }
    Console.WriteLine();
}
