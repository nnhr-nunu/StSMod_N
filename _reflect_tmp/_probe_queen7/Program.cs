using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

// Queen class all methods/properties
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
Console.WriteLine("Queen members:");
foreach (var m in queen.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"  {m.MemberType} {m.Name}");

// QueenBoss encounter
var enc = a.GetType("MegaCrit.Sts2.Core.Models.Encounters.QueenBoss")!;
foreach (var m in enc.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"QueenBoss {m.MemberType} {m.Name}");

// Search CombatState for win - HittableEnemies empty
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
foreach (var m in cs.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (m.Name.Contains("Hittable") || m.Name.Contains("Enemy") && m.Name.Contains("Alive"))
        Console.WriteLine($"CombatState.{m.Name}");
}

// CombatManager - what triggers CheckWinCondition
foreach (var m in a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
    if (m.Name.Contains("CheckWin") || m.Name.Contains("AllEnemy") || m.Name.Contains("Hittable"))
        Console.WriteLine($"CombatManager.{m.Name}");
