using System;
using System.Linq;
using System.Reflection;
class P {
  static void Dump(MethodInfo m) {
    if (m?.GetMethodBody()==null) { Console.WriteLine(m?.DeclaringType?.Name+"."+m?.Name+" no"); return; }
    Console.WriteLine("=== " + m.DeclaringType.Name + "." + m.Name + " ===");
    var il = m.GetMethodBody().GetILAsByteArray();
    var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op == 0x28 || op == 0x6F || op == 0x73) {
        int token = BitConverter.ToInt32(il, i+1);
        try {
          var member = module.ResolveMethod(token);
          Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"newobj") + " " + member.DeclaringType?.Name + "." + member.Name + "(" + string.Join(",", member.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
        } catch {}
        i += 4;
      } else if (op == 0x72) {
        int token = BitConverter.ToInt32(il, i+1);
        try { Console.WriteLine("ldstr \"" + module.ResolveString(token) + "\""); } catch {}
        i += 4;
      }
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var t = a.GetTypes().First(x => x.Name == "VoidFormPower");
    Dump(t.GetMethod("AfterCardPlayed", BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
  }
}
