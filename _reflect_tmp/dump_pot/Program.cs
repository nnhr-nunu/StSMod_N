using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Potion") && t.Name.Contains("Holder"))) {
      Console.WriteLine(t.FullName);
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        if (m.Name.Contains("Input") || m.Name.Contains("Click") || m.Name.Contains("Use"))
          Console.WriteLine("  " + m.Name);
    }
  }
}
