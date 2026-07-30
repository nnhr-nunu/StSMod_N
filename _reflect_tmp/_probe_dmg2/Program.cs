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

int hookIdx = -1;
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            if (member?.Name == "ModifyUnblockedDamageTarget")
            {
                hookIdx = i;
                break;
            }
        }
        catch { }
        i += 4;
    }
}

Console.WriteLine($"ModifyUnblockedDamageTarget at {hookIdx}");
if (hookIdx > 0)
{
    for (var j = Math.Max(0, hookIdx - 30); j < Math.Min(il.Length, hookIdx + 40); j++)
    {
        if (il[j] is 0x28 or 0x6F)
        {
            try
            {
                var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, j + 1));
                Console.WriteLine($"  {j}: {member!.DeclaringType?.Name}.{member.Name}");
            }
            catch { }
            j += 4;
        }
    }
}
