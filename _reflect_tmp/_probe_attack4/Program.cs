using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;

foreach (var m in ac.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
{
    if (m.Name != "Execute") continue;
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo move = m;
    if (attr != null)
        move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = move.GetMethodBody()?.GetILAsByteArray()!;
    
    // find GetPossibleTargets call then next calls
    bool afterGetTargets = false;
    int count = 0;
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (member?.Name == "GetPossibleTargets")
                    afterGetTargets = true;
                if (afterGetTargets && count < 25)
                {
                    Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                    count++;
                }
            }
            catch { }
            i += 4;
        }
    }
}
