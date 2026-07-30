using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cc = a.GetType("MegaCrit.Sts2.Core.Commands.CreatureCmd")!;

foreach (var m in cc.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Damage"))
{
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    if (attr == null) continue;
    var move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = move.GetMethodBody()!.GetILAsByteArray();
    bool has = false;
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            try
            {
                var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (member?.Name == "ModifyUnblockedDamageTarget")
                    has = true;
            }
            catch { }
            i += 4;
        }
    }
    if (has)
        Console.WriteLine($"HAS HOOK: params={m.GetParameters().Length} {string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))}");
}
