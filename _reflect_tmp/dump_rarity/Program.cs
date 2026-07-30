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
      if (op is 0x28 or 0x6F or 0x7B or 0x6A or 0x72) {
        int token = BitConverter.ToInt32(il, i+1);
        try { var member = module.ResolveMember(token); Console.WriteLine((op switch{0x28=>"call",0x6F=>"callvirt",0x7B=>"ldfld",0x6A=>"ldstr",0x72=>"ldstr",_=>"?"}) + " " + member); } catch {}
        i += 4;
      } else if (op == 0x2B) i += 1;
    }
  }
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var inspect = a.GetTypes().First(x => x.Name == "NInspectRelicScreen");
    Dump(inspect.GetMethod("SetRarityVisuals", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    Dump(inspect.GetMethod("_Input", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    var rs = a.GetType("MegaCrit.Sts2.Core.Entities.Relics.RelicStatus");
    Console.WriteLine("RelicStatus: " + string.Join(", ", Enum.GetNames(rs)));
    var ht = a.GetType("MegaCrit.Sts2.Core.HoverTips.HoverTip");
    foreach (var c in ht.GetConstructors()) Console.WriteLine("HoverTip ctor: " + string.Join(",", c.GetParameters().Select(p=>p.ParameterType.Name)));
    foreach (var p in ht.GetProperties()) Console.WriteLine("HoverTip." + p.Name);
  }
}
