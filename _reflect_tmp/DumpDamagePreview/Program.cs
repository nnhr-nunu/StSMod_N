using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

class P {
  static Assembly a;
  static void Main() {
    a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

    // Full decompile-ish of IsPoweredAttack
    var vpe = a.GetType("MegaCrit.Sts2.Core.ValueProps.ValuePropExtensions");
    DumpFull(vpe.GetMethod("IsPoweredAttack"));

    // DamageVar.UpdateCardPreview
    var dv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar");
    DumpFull(dv.GetMethod("UpdateCardPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly));

    // DynamicVar.UpdateCardPreview base
    var dyn = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar");
    foreach (var m in dyn.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
      .Where(x => x.Name.Contains("Preview") || x.Name.Contains("Update")))
      DumpFull(m);

    // CalculatedDamageVar
    var cdv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedDamageVar");
    Console.WriteLine("\n=== CalculatedDamageVar ===");
    Console.WriteLine("Base: " + cdv.BaseType?.FullName);
    foreach (var c in cdv.GetConstructors())
      Console.WriteLine("ctor(" + string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
    foreach (var m in cdv.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      Console.WriteLine("  " + m.Name);

    DumpFull(cdv.GetMethod("UpdateCardPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly));

    // Find how vanilla Strike uses DamageVar - search Strike card
    foreach (var name in new[]{"Strike_Red","Strike","StrikeIronclad","Defend_Red"}) {
      var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
      if (t != null) Console.WriteLine("Found card " + t.FullName);
    }

    // Find any card with CanonicalVars that we can inspect via attributes - better: find DamageVar constructor usage via looking at known cards
    var cards = a.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains("Models.Cards") && t.Name.Contains("Strike")).Take(20);
    Console.WriteLine("\n=== Strike-like cards ===");
    foreach (var t in cards) Console.WriteLine(t.FullName);

    // CardModel.UpdateDynamicVarPreview
    var cm = a.GetType("MegaCrit.Sts2.Core.Models.CardModel");
    DumpFull(cm.GetMethod("UpdateDynamicVarPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance));

    // CreatureCmd damage calc path for preview - look for CalculateDamage / PredictDamage
    Console.WriteLine("\n=== Methods containing CalculateDamage / Predict / PreviewDamage ===");
    foreach (var t in a.GetTypes()) {
      MethodInfo[] methods;
      try { methods = t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly); }
      catch { continue; }
      foreach (var m in methods) {
        if (m.Name.Contains("CalculateDamage", StringComparison.OrdinalIgnoreCase)
         || m.Name.Contains("PreviewDamage", StringComparison.OrdinalIgnoreCase)
         || m.Name.Contains("PredictDamage", StringComparison.OrdinalIgnoreCase)
         || m.Name == "CalcDamage"
         || (m.Name.Contains("ModifyDamage") && t.Name.Contains("Combat")))
          Console.WriteLine(t.FullName + "." + m.Name + "(" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
      }
    }

    // CardPreviewMode enum
    var cpm = a.GetType("MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode");
    if (cpm != null) {
      Console.WriteLine("\n=== CardPreviewMode ===");
      foreach (var n in Enum.GetNames(cpm)) Console.WriteLine("  " + n + " = " + Convert.ToInt64(Enum.Parse(cpm, n)));
    }
  }

  static void DumpFull(MethodInfo m) {
    if (m == null) { Console.WriteLine("(null method)"); return; }
    Console.WriteLine("\n======== " + m.DeclaringType?.Name + "." + m.Name + " (" + string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name)) + ") ========");
    var body = m.GetMethodBody();
    if (body == null) { Console.WriteLine("(no body)"); return; }
    var il = body.GetILAsByteArray()!;
    var module = m.Module;
    // simple opcode dump focusing on calls/branches/constants/ldfld/stfld
    for (int i = 0; i < il.Length; ) {
      byte op = il[i];
      // prefix
      if (op == 0xFE) {
        byte op2 = il[i+1];
        if (op2 == 0x01) { // ceq
          Console.WriteLine($"  [{i:X3}] ceq");
          i += 2; continue;
        }
        Console.WriteLine($"  [{i:X3}] fe {op2:X2}");
        i += 2; continue;
      }
      switch (op) {
        case 0x00: Console.WriteLine($"  [{i:X3}] nop"); i++; break;
        case 0x02: case 0x03: case 0x04: case 0x05: case 0x06: case 0x07: case 0x08: case 0x09:
          Console.WriteLine($"  [{i:X3}] ldarg.{op-0x02}"); i++; break;
        case 0x0A: case 0x0B: case 0x0C: case 0x0D:
          Console.WriteLine($"  [{i:X3}] stloc.{op-0x0A}"); i++; break;
        case 0x06: Console.WriteLine($"  [{i:X3}] ldarg.0"); i++; break;
        case 0x11: Console.WriteLine($"  [{i:X3}] ldloc.s {il[i+1]}"); i+=2; break;
        case 0x12: Console.WriteLine($"  [{i:X3}] ldloca.s {il[i+1]}"); i+=2; break;
        case 0x13: Console.WriteLine($"  [{i:X3}] stloc.s {il[i+1]}"); i+=2; break;
        case 0x14: Console.WriteLine($"  [{i:X3}] ldnull"); i++; break;
        case 0x15: Console.WriteLine($"  [{i:X3}] ldc.i4.m1"); i++; break;
        case 0x16: case 0x17: case 0x18: case 0x19: case 0x1A: case 0x1B: case 0x1C: case 0x1D: case 0x1E:
          Console.WriteLine($"  [{i:X3}] ldc.i4.{op-0x16}"); i++; break;
        case 0x1F: Console.WriteLine($"  [{i:X3}] ldc.i4.s { (sbyte)il[i+1] }"); i+=2; break;
        case 0x20: Console.WriteLine($"  [{i:X3}] ldc.i4 {BitConverter.ToInt32(il,i+1)}"); i+=5; break;
        case 0x21: Console.WriteLine($"  [{i:X3}] ldc.i8 {BitConverter.ToInt64(il,i+1)}"); i+=9; break;
        case 0x22: Console.WriteLine($"  [{i:X3}] ldc.r4 {BitConverter.ToSingle(il,i+1)}"); i+=5; break;
        case 0x23: Console.WriteLine($"  [{i:X3}] ldc.r8 {BitConverter.ToDouble(il,i+1)}"); i+=9; break;
        case 0x25: Console.WriteLine($"  [{i:X3}] dup"); i++; break;
        case 0x26: Console.WriteLine($"  [{i:X3}] pop"); i++; break;
        case 0x28: case 0x6F: case 0x73: {
          int token = BitConverter.ToInt32(il, i+1);
          string kind = op==0x28?"call":op==0x6F?"callvirt":"newobj";
          try {
            var member = module.ResolveMethod(token);
            Console.WriteLine($"  [{i:X3}] {kind} {member.DeclaringType?.Name}.{member.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] {kind} token={token}"); }
          i+=5; break;
        }
        case 0x2A: Console.WriteLine($"  [{i:X3}] ret"); i++; break;
        case 0x2B: Console.WriteLine($"  [{i:X3}] br.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x2C: Console.WriteLine($"  [{i:X3}] brfalse.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x2D: Console.WriteLine($"  [{i:X3}] brtrue.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x2E: Console.WriteLine($"  [{i:X3}] beq.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x2F: Console.WriteLine($"  [{i:X3}] bge.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x30: Console.WriteLine($"  [{i:X3}] bgt.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x31: Console.WriteLine($"  [{i:X3}] ble.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x32: Console.WriteLine($"  [{i:X3}] blt.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x33: Console.WriteLine($"  [{i:X3}] bne.un.s -> {i+2+(sbyte)il[i+1]:X3}"); i+=2; break;
        case 0x38: Console.WriteLine($"  [{i:X3}] br -> {i+5+BitConverter.ToInt32(il,i+1):X3}"); i+=5; break;
        case 0x39: Console.WriteLine($"  [{i:X3}] brfalse -> {i+5+BitConverter.ToInt32(il,i+1):X3}"); i+=5; break;
        case 0x3A: Console.WriteLine($"  [{i:X3}] brtrue -> {i+5+BitConverter.ToInt32(il,i+1):X3}"); i+=5; break;
        case 0x3B: Console.WriteLine($"  [{i:X3}] beq -> {i+5+BitConverter.ToInt32(il,i+1):X3}"); i+=5; break;
        case 0x6A: Console.WriteLine($"  [{i:X3}] conv.i8"); i++; break;
        case 0x6B: Console.WriteLine($"  [{i:X3}] conv.r8"); i++; break;
        case 0x6C: Console.WriteLine($"  [{i:X3}] conv.u4?"); i++; break;
        case 0x7B: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var f = module.ResolveField(token);
            Console.WriteLine($"  [{i:X3}] ldfld {f.DeclaringType?.Name}.{f.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] ldfld token"); }
          i+=5; break;
        }
        case 0x7C: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var f = module.ResolveField(token);
            Console.WriteLine($"  [{i:X3}] ldflda {f.DeclaringType?.Name}.{f.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] ldflda token"); }
          i+=5; break;
        }
        case 0x7D: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var f = module.ResolveField(token);
            Console.WriteLine($"  [{i:X3}] stfld {f.DeclaringType?.Name}.{f.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] stfld token"); }
          i+=5; break;
        }
        case 0x72: {
          int token = BitConverter.ToInt32(il, i+1);
          try { Console.WriteLine($"  [{i:X3}] ldstr \"{module.ResolveString(token)}\""); }
          catch { Console.WriteLine($"  [{i:X3}] ldstr"); }
          i+=5; break;
        }
        case 0x74: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var t = module.ResolveType(token);
            Console.WriteLine($"  [{i:X3}] castclass {t.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] castclass"); }
          i+=5; break;
        }
        case 0x75: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var t = module.ResolveType(token);
            Console.WriteLine($"  [{i:X3}] isinst {t.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] isinst"); }
          i+=5; break;
        }
        case 0x8C: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var t = module.ResolveType(token);
            Console.WriteLine($"  [{i:X3}] box {t.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] box"); }
          i+=5; break;
        }
        case 0xA5: Console.WriteLine($"  [{i:X3}] conv.ovf?"); i++; break;
        case 0xD0: {
          int token = BitConverter.ToInt32(il, i+1);
          try {
            var t = module.ResolveType(token);
            Console.WriteLine($"  [{i:X3}] ldtoken {t.Name}");
          } catch { Console.WriteLine($"  [{i:X3}] ldtoken"); }
          i+=5; break;
        }
        default:
          Console.WriteLine($"  [{i:X3}] op_{op:X2}");
          i++; break;
      }
    }
  }
}
