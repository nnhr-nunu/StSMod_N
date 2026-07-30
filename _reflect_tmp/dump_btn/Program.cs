using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void DumpIL(MethodInfo m) {
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
    var body = m.GetMethodBody(); if (body==null) return;
    var il = body.GetILAsByteArray()!; var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op is 0x28 or 0x6F) {
        int token = BitConverter.ToInt32(il, i+1);
        try { var member = module.ResolveMethod(token); Console.WriteLine((op==0x28?"call":"callvirt") + " " + member.DeclaringType?.Name + "." + member.Name); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    foreach (var name in new[]{"OnPress","_GuiInput","EmitSignalMousePressed"})
      foreach (var m in holder.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy))
        if (m.Name == name && m.DeclaringType?.Name == (name=="_GuiInput"||name=="EmitSignalMousePressed"?"NClickableControl":name=="OnPress"?"NButton":"NRelicInventoryHolder"))
          DumpIL(m);
    var nbtn = a.GetTypes().First(x => x.Name == "NButton");
    DumpIL(nbtn.GetMethod("OnPress", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    DumpIL(a.GetTypes().First(x => x.Name == "NClickableControl").GetMethod("_GuiInput", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
  }
}
