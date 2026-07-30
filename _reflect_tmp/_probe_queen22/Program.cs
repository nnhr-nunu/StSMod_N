using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

void DumpMethod(Type type, string name)
{
    var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .FirstOrDefault(x => x.Name == name);
    if (method == null) { Console.WriteLine($"{type.Name}.{name} NOT FOUND"); return; }
    var attr = method.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = method;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = body.GetMethodBody()?.GetILAsByteArray();
    if (il == null) return;
    Console.WriteLine($"\n{type.Name}.{name}:");
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
                    if (!n.Contains("Async") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("Task") && !n.Contains("SetResult") && !n.Contains("SetException"))
                        Console.WriteLine($"  {n}");
                }
            }
            catch { }
            i += 4;
        }
    }
}

var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
DumpMethod(queen, "AfterDeath");
DumpMethod(torch, "OnDieToDoom");
DumpMethod(torch, "AfterDeath");
DumpMethod(queen, "ShouldStopCombatFromEnding");

var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
var m = cm.GetMethod("get_IsEnding", BindingFlags.Public | BindingFlags.Instance)!;
Console.WriteLine("\nget_IsEnding full IL: " + BitConverter.ToString(m.GetMethodBody()!.GetILAsByteArray()!));

// find Any lambda for IsEnding in CombatManager
foreach (var nested in cm.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var lm in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (!lm.Name.Contains("b__")) continue;
        var il = lm.GetMethodBody()?.GetILAsByteArray();
        if (il == null || il.Length > 30) continue;
        bool enemy = false;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = lm.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    if (member?.Name.Contains("Alive") == true || member?.Name.Contains("Dead") == true || member?.Name.Contains("Hittable") == true)
                    {
                        Console.WriteLine($"{nested.Name}.{lm.Name} -> {member.DeclaringType?.Name}.{member.Name}");
                        enemy = true;
                    }
                }
                catch { }
                i += 4;
            }
        }
    }
}
