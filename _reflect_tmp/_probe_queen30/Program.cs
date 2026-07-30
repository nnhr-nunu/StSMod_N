using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;

foreach (var m in cm.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
{
    if (!m.Name.Contains("b__")) continue;
    var il = m.GetMethodBody()?.GetILAsByteArray();
    if (il == null) continue;
    Console.WriteLine($"{m.Name} ({il.Length}):");
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
