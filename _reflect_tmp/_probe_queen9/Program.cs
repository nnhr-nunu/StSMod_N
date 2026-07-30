using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;

foreach (var name in new[] { "BeforeRemovedFromRoom", "AfterAddedToRoom", "ShouldCreatureBeRemovedFromCombatAfterDeath" })
{
    foreach (var m in queen.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == name))
        Console.WriteLine($"Queen has {name}");
    var mm = a.GetType("MegaCrit.Sts2.Core.Models.MonsterModel")!;
    foreach (var m in mm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == name))
    {
        var il = m.GetMethodBody()?.GetILAsByteArray();
        Console.WriteLine($"MonsterModel.{name} IL len={il?.Length}");
    }
}

// AdaptablePower ShouldStopCombatFromEnding
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
foreach (var m in ap.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == "ShouldStopCombatFromEnding"))
{
    var il = m.GetMethodBody()!.GetILAsByteArray();
    Console.WriteLine("AdaptablePower.ShouldStopCombatFromEnding:");
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
            i += 4;
        }
    }
}

// Hook ShouldCreatureBeRemovedFromCombatAfterDeath default flow
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
foreach (var m in hook.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "ShouldCreatureBeRemovedFromCombatAfterDeath"))
{
    var il = m.GetMethodBody()!.GetILAsByteArray();
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"Hook.ShouldCreatureBeRemovedFromCombatAfterDeath: {member!.DeclaringType?.Name}.{member.Name}");
            i += 4;
        }
    }
}
