using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes().Where(x => x.FullName?.Contains("Combat") == true && x.Name.Contains("State")))
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        if (m.Name.Contains("Victory") || m.Name.Contains("Win") || m.Name.Contains("EndCombat") || m.Name == "CheckForVictory")
            Console.WriteLine($"{t.Name}.{m.Name}");
    }
}

// Search all types for HittableEnemies usage in win context
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
foreach (var m in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = m;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = body.GetMethodBody()?.GetILAsByteArray();
    if (il == null) continue;
    bool hittable = false, stopEnd = false;
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (member?.Name.Contains("Hittable") == true) hittable = true;
                if (member?.Name.Contains("StopCombat") == true) stopEnd = true;
            }
            catch { }
            i += 4;
        }
    }
    if (hittable || stopEnd)
        Console.WriteLine($"CombatManager.{m.Name} hittable={hittable} stopEnd={stopEnd}");
}

// CombatState nested - all lambdas with IsAlive
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        var il = m.GetMethodBody()?.GetILAsByteArray();
        if (il == null) continue;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    if (member?.Name == "get_IsAlive" || member?.Name.Contains("Hittable") == true)
                        Console.WriteLine($"CombatState lambda {nested.Name}.{m.Name} -> {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}
