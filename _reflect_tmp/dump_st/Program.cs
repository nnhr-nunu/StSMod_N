using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
    var st = cm.GetNestedType("<StartTurn>d__106", BindingFlags.NonPublic);
    if (st != null) {
      var m = st.GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
      var il = m!.GetMethodBody()!.GetILAsByteArray()!;
      var module = m.Module;
      for (int i = 0; i < il.Length; i++) {
        if (il[i] is 0x28 or 0x6F) {
          try {
            var member = module.ResolveMethod(BitConverter.ToInt32(il,i+1));
            if (member?.Name.Contains("PlayerActions") == true || member?.Name.Contains("Disabled") == true)
              Console.WriteLine(member.DeclaringType?.Name + "." + member.Name);
          } catch {}
          i += 4;
        } else if (il[i] == 0x2B) i += 1;
      }
    }
  }
}
