using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void DumpIL(MethodInfo m) {
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
    var body = m.GetMethodBody();
    if (body == null) { Console.WriteLine("no body"); return; }
    var il = body.GetILAsByteArray()!;
    var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op is 0x28 or 0x6F) {
        int token = BitConverter.ToInt32(il, i+1);
        try {
          var member = module.ResolveMethod(token);
          Console.WriteLine((op==0x28?"call":"callvirt") + " " + member.DeclaringType?.Name + "." + member.Name);
        } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var typeName in new[]{"NRelicInventory","NInspectRelicScreen","NRelicInventoryHolder","NButton"}) {
      var t = a.GetTypes().First(x => x.Name == typeName);
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        if (m.Name.Contains("Right") || m.Name.Contains("RelicClick") || m.Name == "_GuiInput" || m.Name == "EmitSignalMousePressed")
          DumpIL(m);
    }
    var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager");
    foreach (var p in cm!.GetProperties(BindingFlags.Public|BindingFlags.Instance))
      if (p.Name.Contains("Action") || p.Name.Contains("Progress") || p.Name.Contains("Combat"))
        Console.WriteLine("CombatManager." + p.Name + " : " + p.PropertyType.Name);
  }
}
