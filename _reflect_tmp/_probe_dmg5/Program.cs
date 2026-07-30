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
            var n = $"{member!.DeclaringType?.Name}.{member.Name}";
            if (n.Contains("Hook") || n.Contains("Unblocked") || n.Contains("Dealer") || n.Contains("Receiver") || n == "Creature.op_Equality" || n.Contains("ReferenceEquals"))
                Console.WriteLine(n);
        }
        catch { }
        i += 4;
    }
}
