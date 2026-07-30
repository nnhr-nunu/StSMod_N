using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;

void DumpIl(MethodInfo m, string label)
{
    var il = m.GetMethodBody()?.GetILAsByteArray();
    if (il == null) return;
    Console.WriteLine($"\n{label}:");
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
            }
            catch { }
            i += 4;
        }
    }
}

foreach (var m in ac.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (m.Name is "Targeting" or "TargetingAllOpponents" or "get_IsSingleTargeted")
        DumpIl(m, m.Name);
}
