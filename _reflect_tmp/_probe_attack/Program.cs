using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
Console.WriteLine("Fields:");
foreach (var f in ac.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
    Console.WriteLine($"  {f.Name} : {f.FieldType.Name}");

Console.WriteLine("\nBeforeAttack on PowerModel:");
var pm = a.GetType("MegaCrit.Sts2.Core.Models.PowerModel")!;
foreach (var m in pm.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    if (m.Name.Contains("Attack") || m.Name.Contains("Damage"))
        Console.WriteLine($"  {m.Name}");
