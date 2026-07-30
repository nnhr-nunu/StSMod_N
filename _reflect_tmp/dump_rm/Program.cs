using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var rm = a.GetType("MegaCrit.Sts2.Core.Models.RelicModel")!;
    foreach (var p in rm.GetProperties(BindingFlags.Public|BindingFlags.Instance))
      if (p.Name.Contains("Hover") || p.Name.Contains("Rarity") || p.Name.Contains("Category") || p.Name.Contains("Sort") || p.Name.Contains("Used") || p.Name.Contains("Click"))
        Console.WriteLine(p.Name + " : " + p.PropertyType.Name);
    var inspect = a.GetTypes().First(x => x.Name == "NInspectRelicScreen");
    var open = inspect.GetMethod("Open")!;
    Console.WriteLine("Open params: " + string.Join(", ", open.GetParameters().Select(p=>p.ParameterType.Name)));
    var setR = inspect.GetMethod("SetRarityVisuals", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
    Console.WriteLine("SetRarityVisuals: " + setR);
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    var relProp = holder.GetProperty("Relic")!;
    Console.WriteLine("Relic prop decl: " + relProp.DeclaringType?.Name);
    foreach (var m in holder.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.StartsWith("get_")) Console.WriteLine("prop " + m.Name);
  }
}
