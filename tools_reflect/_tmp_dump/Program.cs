using System; using System.Linq; using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var t = a.GetType("MegaCrit.Sts2.Core.Models.Relics.LuckyFysh")!;
    var m = t.GetMethod("IsAllowed", BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)!;
    Console.WriteLine(m);
    Console.WriteLine(BitConverter.ToString(m.GetMethodBody()!.GetILAsByteArray()!));
    // Shared relic pool
    foreach (var x in a.GetTypes().Where(x => x.Name.Contains("SharedRelic") || x.Name.Contains("EventRelic") || x.Name=="RelicPoolModel").Take(20))
      Console.WriteLine(x.FullName);
  }
}
