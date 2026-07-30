using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var c = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature")!;
foreach (var p in c.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(p => p.Name.Contains("Node") || p.Name.Contains("Visual") || p.Name == "NCreature" || p.PropertyType.Name.Contains("NCreature")))
  Console.WriteLine($"PROP {p.PropertyType.Name} {p.Name}");
foreach (var m in c.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Where(m => m.Name.Contains("Node") || m.Name.Contains("Visual") || m.Name.Contains("Anim")))
{
  var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name+" "+p.Name));
  Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({ps})");
}
// NCombat or similar
foreach (var t in a.GetTypes().Where(t => t.Name is "NCombat" or "CombatManager" || t.Name.Contains("CreatureNode")))
  Console.WriteLine("T "+t.FullName);
