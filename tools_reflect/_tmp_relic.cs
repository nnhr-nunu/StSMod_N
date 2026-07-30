using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var t = a.GetType("MegaCrit.Sts2.Core.Commands.RelicCmd");
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly))
      Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
    Console.WriteLine("--- Player relic remove ---");
    foreach (var name in new[]{"Player","RelicModel","Hook"}) {
      var pt = a.GetTypes().Where(x => x.Name == name || x.Name.Contains("Relic") && x.Name.Contains("Cmd")).Take(5);
    }
    foreach (var mt in a.GetTypes().Where(x => x.Name.Contains("Relic") && (x.Name.Contains("Cmd") || x.Name.Contains("Manager"))))
      Console.WriteLine(mt.FullName);
  }
}
