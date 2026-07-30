using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    // Find methods checking PlayerActionsDisabled before allowing actions
    foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Potion") || t.Name == "CombatManager")) {
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
        if (m.GetMethodBody() == null) continue;
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        bool hasPad = false;
        for (int i=0;i<il.Length-4;i++) {
          if (il[i]==0x28 || il[i]==0x6F) {
            try {
              var mem = m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1));
              if (mem?.Name == "get_PlayerActionsDisabled") { hasPad = true; break; }
            } catch {}
          }
        }
        if (hasPad) Console.WriteLine(t.Name + "." + m.Name);
      }
    }
  }
}
