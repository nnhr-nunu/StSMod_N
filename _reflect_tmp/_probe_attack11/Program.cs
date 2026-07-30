using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
var m = ac.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "Execute");
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
            if (member!.Name.Contains("Attacker") || member.Name.Contains("Receiver") || member.Name == "get_IsPlayer" || member.Name.Contains("Same") || member.Name.Contains("Self"))
                Console.WriteLine($"{member.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
