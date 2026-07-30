using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;

foreach (var name in new[] { "get_HittableEnemies", "get_Enemies", "get_PrimaryEnemies" })
{
    var m = cs.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).FirstOrDefault(x => x.Name == name);
    if (m == null) { Console.WriteLine($"no {name}"); continue; }
    Console.WriteLine($"\n=== CombatState.{name} ===");
    var il = m.GetMethodBody()?.GetILAsByteArray();
    if (il == null) continue;
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

// Find PrimaryEnemy or MinionPower in win check - search CombatManager for Minion or Primary
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
foreach (var m in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
{
    try
    {
        var il = m.GetMethodBody()?.GetILAsByteArray();
        if (il == null) continue;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (member?.Name.Contains("Primary") == true || member?.Name.Contains("Minion") == true || member?.Name == "get_HittableEnemies")
                    Console.WriteLine($"{m.Name}: {member.DeclaringType?.Name}.{member.Name}");
                i += 4;
            }
        }
    }
    catch { }
}
