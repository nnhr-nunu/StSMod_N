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
      if (op is 0x28 or 0x6F) {
        int token = BitConverter.ToInt32(il, i+1);
        try { var member = module.ResolveMethod(token); Console.WriteLine((op==0x28?"call":"callvirt") + " " + member.DeclaringType?.Name + "." + member.Name); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var ph = a.GetTypes().First(x => x.Name == "NPotionHolder");
    foreach (var m in ph.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
      if (m.Name.Contains("Input") || m.Name.Contains("Click") || m.Name.Contains("Press") || m.Name.Contains("Discard") || m.Name == "_Ready")
        if (m.DeclaringType == ph || m.DeclaringType?.Name == "NButton" || m.DeclaringType?.Name == "NClickableControl")
          Dump(m);
  }
}
