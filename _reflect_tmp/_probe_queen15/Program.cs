using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

// Queen ShouldDisappearFromDoom
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var m = queen.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("Queen.ShouldDisappearFromDoom IL: " + BitConverter.ToString(il));

var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
m = torch.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("TorchHead.ShouldDisappearFromDoom IL: " + BitConverter.ToString(il));

// Experiment (Adaptable) ShouldDisappearFromDoom
var exp = a.GetTypes().FirstOrDefault(t => t.Name.Contains("Experiment") && t.Namespace?.Contains("Monsters") == true);
if (exp != null)
{
    m = exp.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    if (m != null)
    {
        il = m.GetMethodBody()!.GetILAsByteArray()!;
        Console.WriteLine($"{exp.Name}.ShouldDisappearFromDoom IL: " + BitConverter.ToString(il));
        foreach (var pm in exp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name.Contains("Respawn") || x.Name.Contains("Adaptable")))
            Console.WriteLine($"  {exp.Name}.{pm.Name}");
    }
}

// AdaptablePower ShouldStopCombatFromEnding IL
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
m = ap.GetMethod("ShouldStopCombatFromEnding", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("AdaptablePower.ShouldStopCombatFromEnding IL: " + BitConverter.ToString(il));

// CombatManager win check
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
foreach (var method in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (method.Name.Contains("Win") || method.Name.Contains("EndCombat") || method.Name.Contains("Check"))
        Console.WriteLine($"CombatManager.{method.Name}");
}
