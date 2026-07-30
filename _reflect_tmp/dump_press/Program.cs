using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void Dump(MethodInfo m) {
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
    var body = m.GetMethodBody(); if (body==null) return;
    var il = body.GetILAsByteArray()!; var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op is 0x28 or 0x6F or 0x7B or 0xFE) {
        int token = BitConverter.ToInt32(il, i+1);
        if (op == 0xFE) { i += 4; continue; }
        try { var member = module.ResolveMember(token); Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"ldfld") + " " + member); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var nc = a.GetTypes().First(x => x.Name == "NClickableControl");
    Dump(nc.GetMethod("OnPressHandler", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    Dump(holder.GetMethod("ConnectSignals", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var rm = a.GetType("MegaCrit.Sts2.Core.Models.RelicModel")!;
    foreach (var m in rm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name is "get_HoverTip" or "get_HoverTips" or "get_Status") Dump(m);
  }
}
