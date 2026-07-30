using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    void DumpIL(MethodInfo m) {
      Console.WriteLine("=== " + m.DeclaringType.Name + "." + m.Name + " ===");
      var body = m.GetMethodBody();
      if (body == null) { Console.WriteLine("no body"); return; }
      var il = body.GetILAsByteArray();
      var module = m.Module;
      // crude: print tokens for call/callvirt
      for (int i = 0; i < il.Length; i++) {
        byte op = il[i];
        if (op == 0x28 || op == 0x6F) { // call / callvirt
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var member = module.ResolveMethod(token);
            Console.WriteLine((op==0x28?"call":"callvirt") + " " + member.DeclaringType?.Name + "." + member.Name);
          } catch {
            try {
              var member = module.ResolveMember(token);
              Console.WriteLine("member " + member);
            } catch { Console.WriteLine("token " + token); }
          }
          i += 4;
        } else if (op == 0x2B) { i += 1; } // br.s
      }
    }
    var inv = a.GetType("MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventory");
    DumpIL(inv.GetMethod("OnRelicClicked", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance));
    var glow = a.GetType("MegaCrit.Sts2.Core.Models.CardModel").GetMethod("get_ShouldGlowGold");
    DumpIL(glow);
    var hf = a.GetType("MegaCrit.Sts2.Core.Models.Relics.HappyFlower");
    foreach (var m in hf.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Click") || m.Name.Contains("Activ") || m.Name.Contains("Use") || m.GetParameters().Length==0 && m.Name.StartsWith("After"))
        Console.WriteLine("HF " + m.Name);
  }
}
