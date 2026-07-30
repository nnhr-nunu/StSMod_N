using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var inspect = a.GetTypes().First(x => x.Name == "NInspectRelicScreen");
    foreach (var m in inspect.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine(m.Name + "(" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
    Console.WriteLine("--- fields ---");
    foreach (var f in inspect.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine(f.FieldType.Name + " " + f.Name);
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    foreach (var m in holder.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Press") || m.Name.Contains("Click") || m.Name.Contains("Input") || m.Name.Contains("Button"))
        Console.WriteLine("HOLDER " + m.Name);
    var btn = a.GetTypes().First(x => x.Name == "NButton");
    foreach (var m in btn.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Press") || m.Name.Contains("Click") || m.Name.Contains("Input") || m.Name.Contains("Button"))
        Console.WriteLine("BUTTON " + m.Name);
  }
}
