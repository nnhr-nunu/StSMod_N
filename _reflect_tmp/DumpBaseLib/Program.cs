using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\\Users\\homut\\AppData\\Local\\Temp\\cursor-sandbox-cache\\335e404fe0aa633de85e793fd668ed45\\nuget\\alchyr.sts2.baselib\\3.3.6\\lib\\net9.0\\BaseLib.dll");
    Console.WriteLine("BaseLib types with Card/Damage/Preview/Dynamic:");
    foreach (var t in a.GetTypes().Where(t =>
        t.Name.Contains("Card") || t.Name.Contains("Damage") || t.Name.Contains("Preview") || t.Name.Contains("Dynamic"))
      .OrderBy(t => t.FullName))
      Console.WriteLine("  " + t.FullName);

    var ccm = a.GetTypes().FirstOrDefault(t => t.Name == "CustomCardModel");
    if (ccm != null) {
      Console.WriteLine("\n=== CustomCardModel methods ===");
      foreach (var m in ccm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        Console.WriteLine("  " + m.Name);
    }
  }
}
