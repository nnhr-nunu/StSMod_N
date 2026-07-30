using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

// Who references AdaptablePower in monster setup?
foreach (var t in a.GetTypes().Where(x => x.Namespace?.Contains("Monsters") == true))
{
    foreach (var mt in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var attr = mt.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
        MethodInfo? mbody = mt;
        if (attr != null)
            mbody = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var mil = mbody?.GetMethodBody()?.GetILAsByteArray();
        if (mil == null) continue;
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x73)
            {
                try
                {
                    var type = mbody!.Module.ResolveType(BitConverter.ToInt32(mil, i + 1));
                    if (type?.Name == "AdaptablePower")
                        Console.WriteLine($"{t.Name}.{mt.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

// Hook.ShouldStopCombatFromEnding
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var m = hook.GetMethod("ShouldStopCombatFromEnding", BindingFlags.Public | BindingFlags.Static)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nHook.ShouldStopCombatFromEnding:");
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

// CombatManager - find method that checks enemies alive
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
foreach (var method in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (!method.Name.Contains("Enemy") && !method.Name.Contains("Win") && !method.Name.Contains("End") && !method.Name.Contains("Victory"))
        continue;
    var attr = method.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = method;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var mil = body.GetMethodBody()?.GetILAsByteArray();
    if (mil == null) continue;
    bool refsHittable = false;
    for (var i = 0; i < mil.Length; i++)
    {
        if (mil[i] is 0x28 or 0x6F)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                if (member?.Name.Contains("Hittable") == true || member?.Name.Contains("Enemy") == true)
                    refsHittable = true;
            }
            catch { }
            i += 4;
        }
    }
    if (refsHittable)
        Console.WriteLine($"CombatManager.{method.Name} refs enemies/hittable");
}
