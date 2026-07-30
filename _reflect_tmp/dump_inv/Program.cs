using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var inv = a.GetTypes().First(x => x.Name == "NRelicInventory");
    foreach (var f in inv.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine(f.FieldType.Name + " " + f.Name);
    foreach (var m in inv.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (!m.Name.StartsWith("get_") && !m.Name.StartsWith("set_") && !m.Name.Contains("Godot"))
        Console.WriteLine(m.Name);
    var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
    foreach (var m in cm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance))
      if (m.Name.Contains("PlayerActions") || m.Name.Contains("Disable") || m.Name.Contains("Enable"))
        Console.WriteLine("CM " + m.Name);
    foreach (var p in cm.GetProperties(BindingFlags.Public|BindingFlags.Instance))
      if (p.Name.Contains("Action") || p.Name.Contains("Disable"))
        Console.WriteLine("CM prop " + p.Name);
  }
}
