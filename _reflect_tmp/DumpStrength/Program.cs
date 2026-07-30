using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var name in new[]{"StrengthPower","VulnerablePower"}) {
      var t = a.GetType("MegaCrit.Sts2.Core.Models.Powers." + name)!;
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)
        .Where(m => m.Name.Contains("ModifyDamage"))) {
        Console.WriteLine("\n=== " + name + "." + m.Name + " ===");
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        var module = m.Module;
        for (int i = 0; i < il.Length; i++) {
          byte op = il[i];
          if (op == 0x28 || op == 0x6F || op == 0x73) {
            try {
              var member = module.ResolveMethod(BitConverter.ToInt32(il, i+1));
              Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"newobj") + " " + member.DeclaringType?.Name + "." + member.Name);
            } catch {}
            i += 4;
          } else if (op == 0x72) {
            try { Console.WriteLine("ldstr \"" + module.ResolveString(BitConverter.ToInt32(il,i+1)) + "\""); } catch {}
            i += 4;
          }
        }
      }
    }
    // AttackCommand field default: create via DamageCmd.Attack and check DamageProps via reflection on instance... can't without running game.
    // Dump DamageCmd.Attack(Decimal) ctor path
    var dm = a.GetType("MegaCrit.Sts2.Core.Commands.DamageCmd")!.GetMethod("Attack", new[]{typeof(decimal)})!;
    Console.WriteLine("\n=== DamageCmd.Attack(Decimal) ===");
    var il2 = dm.GetMethodBody()!.GetILAsByteArray()!;
    var mod2 = dm.Module;
    for (int i = 0; i < il2.Length; i++) {
      byte op = il2[i];
      if (op == 0x28 || op == 0x6F || op == 0x73) {
        try {
          var member = mod2.ResolveMethod(BitConverter.ToInt32(il2, i+1));
          Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"newobj") + " " + member.DeclaringType?.Name + "." + member.Name);
        } catch {}
        i += 4;
      } else if (op is >= 0x16 and <= 0x1E) Console.WriteLine("ldc.i4." + (op-0x16));
      else if (op == 0x1F) { Console.WriteLine("ldc.i4.s " + (sbyte)il2[i+1]); i++; }
      else if (op == 0x20) { Console.WriteLine("ldc.i4 " + BitConverter.ToInt32(il2,i+1)); i+=4; }
      else if (op == 0x7D) {
        try {
          var f = mod2.ResolveField(BitConverter.ToInt32(il2, i+1));
          Console.WriteLine("stfld " + f.DeclaringType?.Name + "." + f.Name);
        } catch {}
        i += 4;
      }
    }
  }
}
