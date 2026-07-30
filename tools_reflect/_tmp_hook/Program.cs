using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var pm = a.GetTypes().First(t => t.Name == "PowerModel");
    foreach (var m in pm.GetMethods(BindingFlags.Public|BindingFlags.Instance).Where(m => m.Name.Contains("Death") || m.Name.Contains("Died")))
      Console.WriteLine(m.Name);
  }
}
