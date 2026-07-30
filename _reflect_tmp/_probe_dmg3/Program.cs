using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cc = a.GetType("MegaCrit.Sts2.Core.Commands.CreatureCmd")!;
var m = cc.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .First(x => x.Name == "Damage" && x.GetParameters().Length == 7);
var attr = m.GetCustomAttributesData().First(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
var move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
var il = move.GetMethodBody()!.GetILAsByteArray();

for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            if (member!.DeclaringType?.Name == "Hook" || member.Name.Contains("Unblocked") || member.Name.Contains("Damage"))
                Console.WriteLine($"{member.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
