using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;

foreach (var nested in ac.GetNestedTypes(BindingFlags.NonPublic))
{
    if (!nested.Name.Contains("Execute") && !nested.Name.Contains("d__")) continue;
    Console.WriteLine($"nested {nested.Name}");
    foreach (var m in nested.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (m.Name.Contains("MoveNext") || m.Name.Contains("b__") || m.Name.StartsWith("<"))
            Console.WriteLine($"  {m.Name}");
    }
}

// search all nested for creature filter
foreach (var nested in ac.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (!m.Name.Contains("b__")) continue;
        var il = m.GetMethodBody()?.GetILAsByteArray();
        if (il == null || il.Length > 80) continue;
        Console.WriteLine($"\n{nested.Name}.{m.Name}:");
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
}
