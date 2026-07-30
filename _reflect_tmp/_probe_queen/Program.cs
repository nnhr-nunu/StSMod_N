using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var name in new[] { "Queen", "TorchHeadAmalgam", "MockAttackAndSummonMinionMonster" })
{
    var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
    if (t == null) { Console.WriteLine($"{name}: NOT FOUND"); continue; }
    Console.WriteLine($"\n=== {t.FullName} ===");
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        if (p.Name.Contains("Disappear") || p.Name.Contains("Flying") || p.Name.Contains("Respawn"))
            Console.WriteLine($"  prop {p.Name}");
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        if (m.Name.Contains("Disappear") || m.Name.Contains("Doom") || m.Name.Contains("Summon") || m.Name.Contains("Minion") || m.Name.Contains("Death"))
            Console.WriteLine($"  {m.Name}");
}

// AdaptablePower on Queen?
var queen = a.GetTypes().FirstOrDefault(x => x.Name == "Queen");
if (queen != null)
{
    Console.WriteLine("\nQueen base: " + queen.BaseType?.Name);
    // search for Adaptable in moves
}

foreach (var t in a.GetTypes().Where(x => x.Name.Contains("Queen")))
    Console.WriteLine("Type: " + t.FullName);

foreach (var t in a.GetTypes().Where(x => x.Name.Contains("TorchHead")))
    Console.WriteLine("Type: " + t.FullName);
