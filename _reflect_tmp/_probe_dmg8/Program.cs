using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cc = a.GetType("MegaCrit.Sts2.Core.Commands.CreatureCmd")!;
// find inner Damage method that might skip self
foreach (var m in cc.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
{
    if (!m.Name.Contains("Damage")) continue;
    var body = m.GetMethodBody();
    if (body == null) continue;
    var il = body.GetILAsByteArray();
    if (il == null) continue;
    for (var i = 0; i < il.Length - 4; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            try
            {
                var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (member?.Name == "ReferenceEquals" || member?.Name == "op_Equality")
                {
                    Console.WriteLine($"{m.Name}({m.GetParameters().Length}): {member.DeclaringType?.Name}.{member.Name}");
                }
            }
            catch { }
        }
    }
}
