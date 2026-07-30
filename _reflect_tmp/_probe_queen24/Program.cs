using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var mp = a.GetType("MegaCrit.Sts2.Core.Models.Powers.MinionPower")!;

foreach (var name in new[] { "ShouldOwnerDeathTriggerFatal", "ShouldPowerBeRemovedAfterOwnerDeath", "ShouldStopCombatFromEnding", "AfterOwnerDied" })
{
    var method = mp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .FirstOrDefault(x => x.Name == name);
    if (method == null) { Console.WriteLine($"MinionPower.{name} NOT FOUND"); continue; }
    var attr = method.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = method;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = body.GetMethodBody()?.GetILAsByteArray();
    if (il == null) continue;
    Console.WriteLine($"\nMinionPower.{name}:");
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                if (il[i] == 0x73)
                {
                    var t = body.Module.ResolveType(BitConverter.ToInt32(il, i + 1));
                    Console.WriteLine($"  TYPE {t?.Name}");
                }
                else
                {
                    var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                    if (!n.Contains("Async") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("Task"))
                        Console.WriteLine($"  {n}");
                }
            }
            catch { }
            i += 4;
        }
    }
}

// Hook ShouldCreatureBeRemovedFromCombatAfterDeath default
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var m = hook.GetMethod("ShouldCreatureBeRemovedFromCombatAfterDeath", BindingFlags.Public | BindingFlags.Static)!;
var hil = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nHook.ShouldCreatureBeRemovedFromCombatAfterDeath:");
for (var i = 0; i < hil.Length; i++)
{
    if (hil[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(hil, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
