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
      if (op is 0x28 or 0x6F or 0x72 or 0x73) {
        int token = BitConverter.ToInt32(il, i+1);
        try { var member = module.ResolveMember(token); Console.WriteLine((op switch{0x72=>"ldstr",0x73=>"ldstr",0x28=>"call",_=>"callvirt"}) + " " + member); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var inv = a.GetTypes().First(x => x.Name == "NRelicInventory");
    Dump(inv.GetMethod("Add", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var ph = a.GetTypes().First(x => x.Name == "NPotionHolder");
    foreach (var n in new[]{"_GuiInput","_Input","UsePotion","OnPress","ConnectSignals","_Ready"})
      Dump(ph.GetMethod(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly));
    // NClickableControl OnPressHandler
    var nc = a.GetTypes().First(x => x.Name == "NClickableControl");
    Dump(nc.GetMethod("OnPressHandler", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
  }
}
