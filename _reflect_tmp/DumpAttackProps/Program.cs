using System;
using System.Linq;
using System.Reflection;

class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

    // AttackCommand / AttackContext - how props are chosen
    foreach (var name in new[]{"AttackCommand","AttackContext","DamageCmd"}) {
      var t = a.GetTypes().First(x => x.Name == name);
      Console.WriteLine("\n=== " + t.FullName + " ===");
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
        Console.WriteLine("  " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
      foreach (var f in t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        Console.WriteLine("  F " + f.FieldType.Name + " " + f.Name);
      foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        Console.WriteLine("  P " + p.PropertyType.Name + " " + p.Name);
    }

    var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
    foreach (var m in ac.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly)
      .Where(m => m.Name is "FromCard" or "Execute" or "WithProps" or "get_Props" or "Attack"))
      DumpCalls(m);

    // AttackContext Execute path
    var ctx = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackContext")!;
    foreach (var m in ctx.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
      .Where(m => m.Name.Contains("Execute") || m.Name.Contains("Damage") || m.Name.Contains("Props")))
      DumpCalls(m);

    // DynamicVar.UpdateCardPreview base
    var dyn = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar")!;
    DumpCalls(dyn.GetMethod("UpdateCardPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);
    DumpCalls(dyn.GetMethod("ToHighlightedString", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);

    // Check how :diff() is parsed - search for method with diff in strings across localization
    Console.WriteLine("\n=== Types/methods with 'diff' string ===");
    foreach (var t in a.GetTypes()) {
      MethodInfo[] methods;
      try { methods = t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly); }
      catch { continue; }
      foreach (var m in methods) {
        var body = m.GetMethodBody();
        if (body == null) continue;
        var il = body.GetILAsByteArray()!;
        var module = m.Module;
        for (int i = 0; i < il.Length-4; i++) {
          if (il[i] != 0x72) continue;
          try {
            var s = module.ResolveString(BitConverter.ToInt32(il, i+1));
            if (s == "diff" || s == ":diff" || s.Contains("diff()", StringComparison.Ordinal))
              Console.WriteLine(t.Name + "." + m.Name + " -> \"" + s + "\"");
          } catch {}
        }
      }
    }
  }

  static void DumpCalls(MethodInfo m) {
    if (m == null) return;
    Console.WriteLine("\n--- " + m.DeclaringType?.Name + "." + m.Name + " ---");
    var body = m.GetMethodBody();
    if (body == null) { Console.WriteLine("(no body)"); return; }
    var il = body.GetILAsByteArray()!;
    var module = m.Module;
    for (int i = 0; i < il.Length; i++) {
      byte op = il[i];
      if (op == 0x28 || op == 0x6F || op == 0x73) {
        int token = BitConverter.ToInt32(il, i+1);
        try {
          var member = module.ResolveMethod(token);
          Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"newobj") + " " + member.DeclaringType?.Name + "." + member.Name);
        } catch {}
        i += 4;
      } else if (op == 0x72) {
        int token = BitConverter.ToInt32(il, i+1);
        try { Console.WriteLine("ldstr \"" + module.ResolveString(token) + "\""); } catch {}
        i += 4;
      } else if (op is >= 0x16 and <= 0x1E) Console.WriteLine("ldc.i4." + (op-0x16));
      else if (op == 0x1F) { Console.WriteLine("ldc.i4.s " + (sbyte)il[i+1]); i++; }
      else if (op == 0x20) { Console.WriteLine("ldc.i4 " + BitConverter.ToInt32(il,i+1)); i+=4; }
      else if (op == 0x14) Console.WriteLine("ldnull");
    }
  }
}
