using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    int n = 0;
    foreach (var t in a.GetTypes()) {
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static)) {
        if (m.GetMethodBody() == null) continue;
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        for (int i=0;i<il.Length-4;i++) {
          if (il[i]==0x28 || il[i]==0x6F) {
            try {
              var mem = m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1));
              if (mem?.Name == "get_PlayerActionsDisabled" && mem.DeclaringType?.Name == "CombatManager") {
                Console.WriteLine(t.Name + "." + m.Name);
                if (++n > 40) return;
              }
            } catch {}
          }
        }
      }
    }
  }
}
