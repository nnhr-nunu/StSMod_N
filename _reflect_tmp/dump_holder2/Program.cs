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
    foreach (var n in new[]{"OnModelChanged","RefreshStatus","_Ready"})
      Dump(holder.GetMethod(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var inv = a.GetTypes().First(x => x.Name == "NRelicInventory");
    Dump(inv.GetMethod("_Input", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var basic = a.GetTypes().First(x => x.Name == "NRelicBasicHolder");
    Console.WriteLine("NRelicBasicHolder base: " + basic.BaseType?.Name);
    foreach (var m in basic.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine("BASIC " + m.Name);
  }
}
