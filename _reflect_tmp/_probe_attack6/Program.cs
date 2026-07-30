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

foreach (var nested in ac.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (m.Name is "b__90_0" or "b__90_2" or "b__90_4")
            DumpIl(m, m.Name);
    }
}

// IsSingleTargeted getter
foreach (var m in ac.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    if (m.Name == "get_IsSingleTargeted")
        DumpIl(m, "get_IsSingleTargeted");
