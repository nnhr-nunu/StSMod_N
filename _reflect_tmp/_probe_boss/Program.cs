using System;
using System.Linq;
using Mono.Cecil;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = AssemblyDefinition.ReadAssembly(path);

void DumpType(string fullName)
{
    var t = asm.MainModule.Types.FirstOrDefault(x => x.FullName == fullName)
        ?? asm.MainModule.Types.SelectMany(x => x.NestedTypes).FirstOrDefault(x => x.FullName == fullName);
    if (t == null) { Console.WriteLine("MISSING " + fullName); return; }
    Console.WriteLine("=== " + t.FullName);
    foreach (var p in t.Properties.OrderBy(p => p.Name))
        Console.WriteLine("  P " + p.Name + " : " + p.PropertyType.FullName);
    foreach (var f in t.Fields.Where(f => !f.Name.Contains("k__BackingField")).OrderBy(f => f.Name))
        Console.WriteLine("  F " + f.Name + " : " + f.FieldType.FullName);
    foreach (var m in t.Methods.Where(m => !m.IsGetter && !m.IsSetter && !m.IsConstructor).OrderBy(m => m.Name))
        if (m.Name.Contains("Room", StringComparison.OrdinalIgnoreCase)
            || m.Name.Contains("Boss", StringComparison.OrdinalIgnoreCase)
            || m.Name.Contains("Elite", StringComparison.OrdinalIgnoreCase)
            || m.Name.Contains("Encounter", StringComparison.OrdinalIgnoreCase)
            || m.Name.Contains("Combat", StringComparison.OrdinalIgnoreCase)
            || m.Name.Contains("Monster", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine("  M " + m.Name + "(" + string.Join(",", m.Parameters.Select(x => x.ParameterType.Name)) + ") : " + m.ReturnType.Name);
}

DumpType("MegaCrit.Sts2.Core.Rooms.RoomType");
DumpType("MegaCrit.Sts2.Core.Rooms.CombatRoom");
DumpType("MegaCrit.Sts2.Core.Entities.Creatures.Creature");
DumpType("MegaCrit.Sts2.Core.Models.EncounterModel");
DumpType("MegaCrit.Sts2.Core.Models.MonsterModel");
DumpType("MegaCrit.Sts2.Core.Combat.CombatState");
DumpType("MegaCrit.Sts2.Core.Combat.ICombatState");

// RoomType fields
var rt = asm.MainModule.Types.First(x => x.FullName == "MegaCrit.Sts2.Core.Rooms.RoomType");
foreach (var f in rt.Fields.Where(f => f.IsStatic))
    Console.WriteLine("RoomType." + f.Name + " = " + f.Constant);
