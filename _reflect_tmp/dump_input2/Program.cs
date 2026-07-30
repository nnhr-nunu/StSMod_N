using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void Dump(MethodInfo? m) {
    if (m==null) { Console.WriteLine("null"); return; }
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ret=" + m.ReturnType.Name + " ===");
    var body = m.GetMethodBody(); if (body==null) return;
    var il = body.GetILAsByteArray()!; var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op is 0x28 or 0x6F or 0x7B) {
        int token = BitConverter.ToInt32(il, i+1);
        try { var member = module.ResolveMember(token); Console.WriteLine((op==0x7B?"ldfld":"call") + " " + member); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    Dump(holder.GetMethod("ConnectSignals", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly));
    var inv = a.GetTypes().First(x => x.Name == "NRelicInventory");
    Dump(inv.GetMethod("OnRelicClicked", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly));
    var btn = a.GetTypes().First(x => x.Name == "NButton");
    foreach (var m in btn.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Press") || m.Name.Contains("Signal") || m.Name == "_GuiInput")
        Dump(m);
    // Find combat relic UI nodes
    foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Relic") && t.Namespace != null && t.Namespace.Contains("Nodes"))) {
      if (t.Name.Contains("Combat") || t.Name.Contains("TopBar") || t.Name.Contains("Hud") || t.Name.Contains("Player"))
        Console.WriteLine("TYPE " + t.Name + " : " + t.BaseType?.Name);
    }
  }
}
