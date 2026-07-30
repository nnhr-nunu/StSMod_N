using System;
using System.Linq;
using System.Reflection;

class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

    DumpCalls(a.GetType("MegaCrit.Sts2.Core.ValueProps.ValuePropExtensions")!.GetMethod("IsPoweredAttack")!);

    var dv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar")!;
    DumpCalls(dv.GetMethod("UpdateCardPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!);

    var cdv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedDamageVar")!;
    Console.WriteLine("\n=== CalculatedDamageVar members ===");
    Console.WriteLine("Base: " + cdv.BaseType?.FullName);
    foreach (var c in cdv.GetConstructors())
      Console.WriteLine("ctor(" + string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
    foreach (var m in cdv.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine("  M " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
    var cdvPreview = cdv.GetMethod("UpdateCardPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);
    if (cdvPreview != null) DumpCalls(cdvPreview);

    var cm = a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!;
    DumpCalls(cm.GetMethod("UpdateDynamicVarPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!);

    // Look for CombatManager / DamageCalculator style
    Console.WriteLine("\n=== Calculate/Modify damage related ===");
    foreach (var t in a.GetTypes()) {
      MethodInfo[] methods;
      try { methods = t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly); }
      catch { continue; }
      foreach (var m in methods) {
        var n = m.Name;
        if (n.Contains("CalculateDamage", StringComparison.OrdinalIgnoreCase)
         || n.Contains("CalcDamage", StringComparison.OrdinalIgnoreCase)
         || n.Contains("PreviewDamage", StringComparison.OrdinalIgnoreCase)
         || n.Contains("GetModifiedDamage", StringComparison.OrdinalIgnoreCase)
         || n == "ModifyDamage"
         || n == "ApplyDamageModifiers"
         || (n.Contains("Damage") && n.Contains("Preview")))
          Console.WriteLine(t.Name + "." + n + "(" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
      }
    }

    // Vanilla Strike card - inspect CanonicalVars via compiled method if exists
    Console.WriteLine("\n=== Looking for Ironclad Strike ===");
    foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Strike") && t.Namespace != null && t.Namespace.Contains("Cards")).Take(30))
      Console.WriteLine(t.FullName);

    // CardPreviewMode
    var cpm = a.GetType("MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode");
    if (cpm != null) {
      Console.WriteLine("\n=== CardPreviewMode ===");
      foreach (var n in Enum.GetNames(cpm!))
        Console.WriteLine("  " + n + " = " + Convert.ToInt64(Enum.Parse(cpm, n)));
    }

    // Check DamageVar props default and fields
    Console.WriteLine("\n=== DamageVar fields/props ===");
    foreach (var f in dv.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
      Console.WriteLine("  F " + f.FieldType.Name + " " + f.Name);
    foreach (var p in dv.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine("  P " + p.PropertyType.Name + " " + p.Name);

    // IntValue getter on DynamicVar - does it use PreviewValue?
    var dyn = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar")!;
    DumpCalls(dyn.GetMethod("get_IntValue")!);
    DumpCalls(dyn.GetMethod("get_PreviewValue")!);
    var setPreview = dyn.GetMethod("set_PreviewValue");
    if (setPreview != null) DumpCalls(setPreview);

    // Find methods that set PreviewValue or call ModifyDamageMultiplicative from damage vars
    Console.WriteLine("\n=== Types calling ModifyDamageMultiplicative (by method name search on Damage*) ===");
    foreach (var tName in new[]{"DamageVar","CalculatedDamageVar","ExtraDamageVar","OstyDamageVar","CreatureCmd","DamageCmd","CombatManager","Hook","PowerHooks"}) {
      var t = a.GetTypes().FirstOrDefault(x => x.Name == tName);
      if (t == null) continue;
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly)) {
        var body = m.GetMethodBody();
        if (body == null) continue;
        var il = body.GetILAsByteArray()!;
        var module = m.Module;
        for (int i = 0; i < il.Length-4; i++) {
          if (il[i] == 0x28 || il[i] == 0x6F) {
            try {
              var member = module.ResolveMethod(BitConverter.ToInt32(il, i+1));
              if (member.Name.Contains("ModifyDamage") || member.Name.Contains("CalculateDamage") || member.Name.Contains("Preview"))
                Console.WriteLine(t.Name + "." + m.Name + " -> " + member.DeclaringType?.Name + "." + member.Name);
            } catch {}
          }
        }
      }
    }
  }

  static void DumpCalls(MethodInfo m) {
    Console.WriteLine("\n======== " + m.DeclaringType?.Name + "." + m.Name + " ========");
    var body = m.GetMethodBody();
    if (body == null) { Console.WriteLine("(no body / abstract)"); return; }
    var il = body.GetILAsByteArray()!;
    var module = m.Module;
    Console.WriteLine("IL length=" + il.Length + " locals=" + body.LocalVariables.Count);
    foreach (var loc in body.LocalVariables)
      Console.WriteLine("  local: " + loc.LocalType.Name);
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
      } else if (op is >= 0x16 and <= 0x1E) {
        Console.WriteLine("ldc.i4." + (op-0x16));
      } else if (op == 0x1F) {
        Console.WriteLine("ldc.i4.s " + (sbyte)il[i+1]); i++;
      } else if (op == 0x20) {
        Console.WriteLine("ldc.i4 " + BitConverter.ToInt32(il, i+1)); i += 4;
      } else if (op == 0x22) {
        Console.WriteLine("ldc.r4 " + BitConverter.ToSingle(il, i+1)); i += 4;
      } else if (op == 0x23) {
        Console.WriteLine("ldc.r8 " + BitConverter.ToDouble(il, i+1)); i += 8;
      } else if (op == 0x2C) {
        Console.WriteLine("brfalse.s");
      } else if (op == 0x2D) {
        Console.WriteLine("brtrue.s");
      } else if (op == 0x2A) {
        Console.WriteLine("ret");
      } else if (op == 0x14) {
        Console.WriteLine("ldnull");
      } else if (op == 0x25) {
        Console.WriteLine("dup");
      }
    }
  }
}
