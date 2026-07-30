using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var t in a.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains("Relics"))) {
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
        if (m.GetMethodBody() == null) continue;
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        for (int i=0;i<il.Length-4;i++) {
          if (il[i]==0x72 || il[i]==0x73) {
            try {
              var s = m.Module.ResolveString(BitConverter.ToInt32(il,i+1));
              if (s != null && (s.Contains("Event") || s.Contains("event") || s.Contains("rarity") || s.Contains("section")))
                Console.WriteLine(t.Name + "." + m.Name + ": " + s);
            } catch {}
          }
        }
      }
    }
  }
}
