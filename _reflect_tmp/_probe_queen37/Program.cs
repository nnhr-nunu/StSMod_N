using System;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var c = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
foreach (var name in new[] { "get_IsSecondaryEnemy", "get_IsPrimaryEnemy", "get_IsAlive", "get_IsDead" })
{
    var m = c.GetMethod(name, BindingFlags.Public | BindingFlags.Instance)!;
  Console.WriteLine($"{name}: {BitConverter.ToString(m.GetMethodBody()!.GetILAsByteArray()!)}");
}
