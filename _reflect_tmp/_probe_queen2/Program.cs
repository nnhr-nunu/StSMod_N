using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

void DumpProp(Type t, string propName)
{
    var p = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
         ?? t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
    if (p == null) { Console.WriteLine($"{t.Name}.{propName}: no property"); return; }
    var m = p.GetGetMethod(true);
    if (m == null) return;
    if (!m.IsVirtual && m.GetMethodBody()?.GetILAsByteArray()?.Length <= 2)
    {
        Console.WriteLine($"{t.Name}.{propName}: auto-property default");
        return;
    }
    var il = m.GetMethodBody()?.GetILAsByteArray();
    Console.WriteLine($"{t.Name}.{propName} IL len={il?.Length}");
    if (il != null && il.Length < 20)
    {
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                i += 4;
            }
            else Console.Write($"{il[i]:X2} ");
        }
        Console.WriteLine();
    }
}

var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
var monster = a.GetType("MegaCrit.Sts2.Core.Models.MonsterModel")!;

DumpProp(queen, "ShouldDisappearFromDoom");
DumpProp(torch, "ShouldDisappearFromDoom");
DumpProp(monster, "ShouldDisappearFromDoom");

// Hook ShouldStopCombatFromEnding
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
foreach (var m in hook.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "ShouldStopCombatFromEnding"))
{
    var il = m.GetMethodBody()!.GetILAsByteArray();
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"ShouldStopCombatFromEnding: {member!.DeclaringType?.Name}.{member.Name}");
            i += 4;
        }
    }
}

// CombatState check win
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
foreach (var m in cs.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    if (m.Name.Contains("End") || m.Name.Contains("Win") || m.Name.Contains("Over") || m.Name.Contains("Alive"))
        Console.WriteLine($"CombatState.{m.Name}");
