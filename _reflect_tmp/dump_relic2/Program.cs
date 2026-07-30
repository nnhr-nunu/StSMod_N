using System;
using System.Linq;
using System.Reflection;

class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var typeName in new[]{"NRelicInventory","NRelicInventoryHolder","NClickableControl","NButton"}) {
      var t = a.GetTypes().First(x => x.Name == typeName);
      Console.WriteLine("=== " + typeName + " : " + t.FullName + " ===");
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(x=>x.Name))
        if (m.Name.Contains("Input") || m.Name.Contains("Press") || m.Name.Contains("Click") || m.Name.Contains("Gui") || m.Name.Contains("Ready") || m.Name.Contains("Relic"))
          Console.WriteLine(m.Name + " :: " + string.Join(", ", m.GetParameters().Select(p=>p.ParameterType.Name)));
    }
  }
}
