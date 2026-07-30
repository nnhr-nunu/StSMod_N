using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var doom = a.GetType("MegaCrit.Sts2.Core.Models.Powers.DoomPower")!;
Console.WriteLine("=== DoomPower methods ===");
foreach (var m in doom.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly)
    .OrderBy(m => m.Name))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine($"{(m.IsStatic?"static ":"")}{m.ReturnType.Name} {m.Name}({ps})");
}

Console.WriteLine("\n=== DoomPower props/fields ===");
foreach (var p in doom.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    Console.WriteLine($"prop {p.PropertyType.Name} {p.Name}");
foreach (var f in doom.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    Console.WriteLine($"field {f.FieldType.Name} {f.Name}");

void DumpCalls(MethodInfo method)
{
    Console.WriteLine($"\n--- IL calls in {method.DeclaringType?.Name}.{method.Name} ---");
    var body = method.GetMethodBody();
    if (body == null) { Console.WriteLine("  (no body)"); return; }
    var il = body.GetILAsByteArray()!;
    var module = method.Module;
    for (var i = 0; i < il.Length; i++)
    {
        var op = il[i];
        if (op is 0x28 or 0x6F or 0x73)
        {
            var token = BitConverter.ToInt32(il, i + 1);
            try {
                var member = module.ResolveMethod(token);
                Console.WriteLine($"  {(op==0x28?"call":op==0x6F?"callvirt":"newobj")} {member!.DeclaringType?.FullName}.{member.Name}");
            } catch {}
            i += 4;
        }
        else if (op == 0x72)
        {
            var token = BitConverter.ToInt32(il, i + 1);
            try { Console.WriteLine("  ldstr \"" + module.ResolveString(token) + "\""); } catch {}
            i += 4;
        }
    }
}

foreach (var name in new[]{"DoomKill","IsOwnerDoomed","AfterSideTurnStart","BeforeSideTurnStart","AfterTurnStart","BeforeTurnStart","OnTurnStart","AfterPlayerTurnStart","ModifyDamage","AfterApplied","BeforePowerApplied"})
{
    var methods = doom.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static)
        .Where(m => m.Name == name && m.DeclaringType == doom).ToList();
    if (methods.Count == 0)
    {
        // try base overrides by name only on doom type declared
        methods = doom.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly)
            .Where(m => m.Name.Contains("Turn", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Doom", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Death", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Kill", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Damage", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    foreach (var m in methods.Distinct()) DumpCalls(m);
}

// Also dump ALL declared methods IL briefly
foreach (var m in doom.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    DumpCalls(m);

Console.WriteLine("\n=== Hook death-related ===");
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
foreach (var m in hook.GetMethods(BindingFlags.Public|BindingFlags.Static)
    .Where(m => m.Name.Contains("Death", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Die", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Kill", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("CombatEnd", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("CombatStart", StringComparison.OrdinalIgnoreCase))
    .OrderBy(m => m.Name))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine($"{m.ReturnType.Name} {m.Name}({ps})");
}

Console.WriteLine("\n=== Creature death-related ===");
var creature = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
foreach (var m in creature.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
    .Where(m => m.Name.Contains("Death", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Die", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Kill", StringComparison.OrdinalIgnoreCase) || m.Name == "LoseHp" || m.Name == "Damage" || m.Name.Contains("Hp"))
    .OrderBy(m => m.Name))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine($"{m.ReturnType.Name} {m.Name}({ps})");
}
