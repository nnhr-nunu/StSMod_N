using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var name in new[]{"NClickableControl","NRelicInventoryHolder","NRelic"}) {
      var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
      if (t == null) { Console.WriteLine(name + " NOT FOUND"); continue; }
      Console.WriteLine("=== " + t.FullName + " : base=" + t.BaseType?.FullName);
      for (var cur = t; cur != null && cur.Name != "Object"; cur = cur.BaseType) {
        foreach (var m in cur.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
          .Where(m => m.Name.Contains("Input", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Click", StringComparison.OrdinalIgnoreCase) || m.Name.Contains("Gui", StringComparison.OrdinalIgnoreCase)))
          Console.WriteLine("  [" + cur.Name + "] " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
      }
    }
  }
}
