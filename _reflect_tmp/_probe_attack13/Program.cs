using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
var m = ac.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).First(x => x.Name == "Targeting" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.Name == "Creature");
var il = m.GetMethodBody()!.GetILAsByteArray();
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F or 0x73)
    {
        var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
        Console.WriteLine($"{member!.DeclaringType?.Name}.{member.Name}");
        i += 4;
    }
}
