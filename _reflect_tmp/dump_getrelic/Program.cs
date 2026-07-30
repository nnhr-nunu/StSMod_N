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
      if (op is 0x28 or 0x6F or 0x7B or 0x7D) {
        int token = BitConverter.ToInt32(il, i+1);
        try {
          var member = module.ResolveMember(token);
          Console.WriteLine(op switch { 0x28=>"call", 0x6F=>"callvirt", 0x7B=>"ldfld", 0x7D=>"stfld", _=>"?" } + " " + member);
        } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var holder = a.GetTypes().First(x => x.Name == "NRelicInventoryHolder");
    Dump(holder.GetMethod("get_Relic", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    foreach (var t in new[]{holder, a.GetTypes().First(x=>x.Name=="NButton"), a.GetTypes().First(x=>x.Name=="NClickableControl")})
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        if (m.Name == "_GuiInput") Dump(m);
    var rr = a.GetType("MegaCrit.Sts2.Core.Entities.Relics.RelicRarity");
    Console.WriteLine("RelicRarity values: " + string.Join(", ", Enum.GetNames(rr)));
  }
}
