using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;

foreach (var nested in cm.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (!m.Name.Contains("IsEnding")) continue;
        var mil = m.GetMethodBody()!.GetILAsByteArray()!;
        Console.WriteLine($"{nested.Name}.{m.Name}:");
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

// Creature.get_IsHittable
var c = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
var ih = c.GetMethod("get_IsHittable", BindingFlags.Public | BindingFlags.Instance)!;
var il = ih.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nCreature.get_IsHittable:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = ih.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}

// AdaptablePower ShouldStopCombatFromEnding - get full method body from PowerModel base?
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
var m2 = ap.GetMethod("ShouldStopCombatFromEnding", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
il = m2.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nAdaptablePower.ShouldStopCombatFromEnding bytes: " + BitConverter.ToString(il));

// Experiment ShouldDisappearFromDoom
foreach (var t in a.GetTypes().Where(x => x.Name.Contains("TestSubject") || x.Name.Contains("Experiment")))
{
    if (!t.Namespace?.Contains("Monsters") == true) continue;
    var sd = t.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    if (sd != null)
    {
        il = sd.GetMethodBody()!.GetILAsByteArray()!;
        Console.WriteLine($"{t.Name}.ShouldDisappearFromDoom: " + BitConverter.ToString(il));
    }
}
