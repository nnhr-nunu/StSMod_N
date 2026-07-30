using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes().Where(x => x.FullName?.Contains("CombatManager") == true))
{
    foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (m.Name.Contains("99_0"))
        {
            var il = m.GetMethodBody()!.GetILAsByteArray()!;
            Console.WriteLine($"{t.FullName}.{m.Name}:");
            for (var i = 0; i < il.Length; i++)
            {
                if (il[i] is 0x28 or 0x6F)
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
}
