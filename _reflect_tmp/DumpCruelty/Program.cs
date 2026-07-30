using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var frailVar = asm.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.PowerVar`1").MakeGenericType(asm.GetType("MegaCrit.Sts2.Core.Models.Powers.FrailPower"));
    var inst = Activator.CreateInstance(frailVar, 2M);
    foreach (var p in inst.GetType().GetProperties()) {
      try { Console.WriteLine($"{p.Name}={p.GetValue(inst)}"); } catch {}
    }
  }
}
