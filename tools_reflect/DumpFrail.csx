using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var frail = a.GetType("MegaCrit.Sts2.Core.Models.Powers.FrailPower");
Console.WriteLine("FrailPower: " + frail?.FullName);
if (frail != null) {
  foreach (var m in frail.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly|BindingFlags.NonPublic))
    Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
  foreach (var p in frail.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
    Console.WriteLine("  prop " + p.PropertyType.Name + " " + p.Name);
}
// base PowerModel Apply targets / IsValidTarget
var pm = a.GetType("MegaCrit.Sts2.Core.Models.PowerModel");
foreach (var m in pm.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m => m.Name.Contains("Apply") || m.Name.Contains("Valid") || m.Name.Contains("Target") || m.Name.Contains("Enemy") || m.Name.Contains("Player")))
  Console.WriteLine("PowerModel." + m.Name);
// FrailPower ModifyBlock or similar
foreach (var t in new[]{"FrailPower","WeakPower","VulnerablePower"}) {
  var ty = a.GetType("MegaCrit.Sts2.Core.Models.Powers." + t);
  Console.WriteLine("=== " + t + " ===");
  if (ty == null) continue;
  var cur = ty;
  while (cur != null && cur.Name != "Object") {
    foreach (var m in cur.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly|BindingFlags.NonPublic)
      .Where(m => m.Name.Contains("Block") || m.Name.Contains("Damage") || m.Name.Contains("Modify") || m.Name.Contains("CanApply") || m.Name.Contains("Owner")))
      Console.WriteLine("  " + cur.Name + "." + m.Name + " -> " + m.ReturnType.Name);
    cur = cur.BaseType;
  }
}
