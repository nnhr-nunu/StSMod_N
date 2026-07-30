using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
using var fs = File.OpenRead(dll);
using var pe = new PEReader(fs);
var mr = pe.GetMetadataReader();

// Search user strings for method order clues
foreach (var h in mr.TypeDefinitions)
{
    var t = mr.GetTypeDefinition(h);
    var name = mr.GetString(t.Name);
    var ns = mr.GetString(t.Namespace);
    if (name is "CombatManager" or "CombatState" or "TurnManager" or "Combat" or "PlayerCombatState" or "CombatSideTurn" or "CombatCoordinator")
        Console.WriteLine(ns + "." + name);
}

var a = Assembly.LoadFrom(dll);
var interesting = a.GetTypes().Where(t =>
    t.Name.Contains("Combat", StringComparison.OrdinalIgnoreCase) &&
    (t.Name.Contains("Manager") || t.Name.Contains("Turn") || t.Name.Contains("Loop") || t.Name.Contains("Runner") || t.Name.Contains("Controller") || t.Name.Contains("Phase"))
).Take(40);
foreach (var t in interesting) Console.WriteLine("TYPE " + t.FullName);

// Look for methods that call both AfterSideTurnStart and AfterPlayerTurnStart - scan IL for MemberRefs
Console.WriteLine("\n--- scanning IL for methods referencing both hooks ---");
foreach (var h in mr.MethodDefinitions)
{
    var m = mr.GetMethodDefinition(h);
    var body = m.RelativeVirtualAddress == 0 ? default : pe.GetMethodBody(m.RelativeVirtualAddress);
    if (body.LocalVariables == null && m.RelativeVirtualAddress == 0) continue;
    if (m.RelativeVirtualAddress == 0) continue;
    try {
        var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILAsByteArray();
        if (il == null || il.Length < 10) continue;
        // too heavy to fully decode; instead search method names containing TurnStart
    } catch {}
}

// Simpler: dump methods named *TurnStart* on combat-related types
foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    {
        if (!m.Name.Contains("TurnStart", StringComparison.OrdinalIgnoreCase) &&
            !m.Name.Contains("StartTurn", StringComparison.OrdinalIgnoreCase) &&
            !m.Name.Contains("BeginTurn", StringComparison.OrdinalIgnoreCase))
            continue;
        Console.WriteLine(t.FullName + "." + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    }
}
