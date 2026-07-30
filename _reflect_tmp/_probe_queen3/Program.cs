using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;

void DumpMethod(Type t, string name)
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(x => x.Name == name))
    {
        Console.WriteLine($"\n=== {t.Name}.{name} ===");
        var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
        MethodInfo body = m;
        if (attr != null)
            body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
        var il = body.GetMethodBody()?.GetILAsByteArray();
        if (il == null) return;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F or 0x73)
            {
                try
                {
                    var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                    if (!n.Contains("AsyncTask") && !n.Contains("TaskAwaiter") && !n.Contains("SetResult") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("MoveNext") && !n.Contains("Dispose"))
                        Console.WriteLine($"  {n}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

DumpMethod(queen, "AfterDeath");
DumpMethod(torch, "OnDieToDoom");

// Does TorchHeadAmalgam reference AdaptablePower?
foreach (var t in new[] { queen, torch })
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
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
                    if (member?.DeclaringType?.Name == "AdaptablePower")
                        Console.WriteLine($"{t.Name}.{m.Name} uses AdaptablePower");
                    i += 4;
                }
            }
        }
        catch { }
    }
}

// CombatManager end check
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
foreach (var m in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    if (m.Name.Contains("End") || m.Name.Contains("Over") || m.Name.Contains("Win"))
        Console.WriteLine($"CombatManager.{m.Name}");
