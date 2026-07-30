using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
    .Where(x => x.Name.Contains("HittableEnemies")))
{

var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine($"Hittable filter {m.Name}:");
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

// AdaptablePower ShouldStopCombatFromEnding
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
foreach (var method in ap.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (!method.Name.Contains("Stop") && !method.Name.Contains("Disappear") && !method.Name.Contains("Reviv") && !method.Name.Contains("Die"))
        continue;
    var attr = method.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = method;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var mil = body.GetMethodBody()?.GetILAsByteArray();
    if (mil == null) continue;
    Console.WriteLine($"\nAdaptablePower.{method.Name}:");
    for (var i = 0; i < mil.Length; i++)
    {
        if (mil[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                if (!n.Contains("Async") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("Task"))
                    Console.WriteLine($"  {n}");
            }
            catch { }
            i += 4;
        }
    }
}

// Queen AfterDeath
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
foreach (var method in queen.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (!method.Name.Contains("Death") && !method.Name.Contains("Removed") && !method.Name.Contains("Die"))
        continue;
    var attr = method.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = method;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var mil = body.GetMethodBody()?.GetILAsByteArray();
    if (mil == null) continue;
    Console.WriteLine($"\nQueen.{method.Name}:");
    for (var i = 0; i < mil.Length; i++)
    {
        if (mil[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                if (!n.Contains("Async") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("Task"))
                    Console.WriteLine($"  {n}");
            }
            catch { }
            i += 4;
        }
    }
}
