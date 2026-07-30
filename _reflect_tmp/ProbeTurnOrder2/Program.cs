using System;
using System.Linq;
using System.Reflection;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var a = Assembly.LoadFrom(dll);

foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    {
        if (!m.Name.Contains("TurnStart", StringComparison.OrdinalIgnoreCase) &&
            !m.Name.Contains("StartTurn", StringComparison.OrdinalIgnoreCase) &&
            !m.Name.Contains("BeginTurn", StringComparison.OrdinalIgnoreCase) &&
            !m.Name.Contains("PlayerTurn", StringComparison.OrdinalIgnoreCase))
            continue;
        Console.WriteLine(t.FullName + "." + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    }
}

Console.WriteLine("\n--- PowerCmd.Apply overloads ---");
var pc = a.GetType("MegaCrit.Sts2.Core.Commands.PowerCmd");
foreach (var m in pc!.GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m => m.Name == "Apply"))
    Console.WriteLine(m + " | params=" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + (p.HasDefaultValue ? "=default" : "") + (Nullable.GetUnderlyingType(p.ParameterType)!=null || p.ParameterType.Name.EndsWith("?") ? "?" : ""))));

Console.WriteLine("\n--- relics applying powers at combat/turn ---");
foreach (var t in a.GetTypes().Where(t => t.Namespace?.Contains("Relics")==true))
{
    var methods = t.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
    var hits = methods.Where(m => m.Name is "AfterPlayerTurnStart" or "BeforeCombatStart" or "AfterCreatureAddedToCombat" or "BeforeSideTurnStart").ToList();
    if (hits.Count == 0) continue;
    Console.WriteLine(t.Name + ": " + string.Join(", ", hits.Select(h => h.Name)));
}
