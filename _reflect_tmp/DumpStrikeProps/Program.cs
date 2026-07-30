using System;
using System.Linq;
using System.Reflection;

class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

    // StrikeIronclad - find CanonicalVars property/method and dump IL for ValueProp used
    foreach (var name in new[]{"StrikeIronclad","PommelStrike","TwinStrike","Bash","Anger"}) {
      var t = a.GetType("MegaCrit.Sts2.Core.Models.Cards." + name);
      if (t == null) { Console.WriteLine("missing " + name); continue; }
      Console.WriteLine("\n===== " + name + " =====");
      DumpCanonicalVars(t);
    }

    // DamageCmd.Attack builder - find AttackBuilder or similar
    Console.WriteLine("\n=== Types with Attack in Commands ===");
    foreach (var t in a.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains("Commands") && (t.Name.Contains("Attack") || t.Name.Contains("Damage"))))
      Console.WriteLine(t.FullName);

    // Hook.ModifyDamage signature details
    var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook") ?? a.GetTypes().First(t => t.Name == "Hook");
    Console.WriteLine("\nHook type: " + hook.FullName);
    foreach (var m in hook.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).Where(m => m.Name.Contains("ModifyDamage")))
      Console.WriteLine(m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");

    var mdht = a.GetTypes().First(t => t.Name == "ModifyDamageHookType");
    Console.WriteLine("\n=== ModifyDamageHookType ===");
    foreach (var n in Enum.GetNames(mdht))
      Console.WriteLine("  " + n + " = " + Convert.ToInt64(Enum.Parse(mdht, n)));

    // Dump Hook.ModifyDamage body calls focusing on null target early-out
    var md = hook.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).First(m => m.Name == "ModifyDamage" && m.GetParameters().Length >= 10);
    DumpCalls(md);
    var mdi = hook.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).First(m => m.Name == "ModifyDamageInternal");
    DumpCalls(mdi);

    // Localization: how {Damage:diff()} resolves - look for DynamicVar formatting
    Console.WriteLine("\n=== DynamicVar format / diff related ===");
    var dyn = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar")!;
    foreach (var m in dyn.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Format") || m.Name.Contains("Diff") || m.Name.Contains("String") || m.Name.Contains("Preview") || m.Name.Contains("Display") || m.Name.Contains("Text"))
        Console.WriteLine("  " + m.Name);

    // Search for "diff" string in DynamicVar / localization
    foreach (var tName in new[]{"DynamicVar","DynamicVarSet","LocString","CardModel","SmartNumber","ColoredNumber"}) {
      var t = a.GetTypes().FirstOrDefault(x => x.Name == tName);
      if (t == null) continue;
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly)) {
        var body = m.GetMethodBody();
        if (body == null) continue;
        var il = body.GetILAsByteArray()!;
        var module = m.Module;
        for (int i = 0; i < il.Length-4; i++) {
          if (il[i] == 0x72) {
            try {
              var s = module.ResolveString(BitConverter.ToInt32(il, i+1));
              if (s.Contains("diff", StringComparison.OrdinalIgnoreCase) || s.Contains("Preview", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine(t.Name + "." + m.Name + " ldstr \"" + s + "\"");
            } catch {}
          }
        }
      }
    }

    // Bash is classic Vulnerable applicator - check its DamageVar props
  }

  static void DumpCanonicalVars(Type t) {
    // property get_CanonicalVars
    var m = t.GetMethod("get_CanonicalVars", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
         ?? t.GetProperty("CanonicalVars", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)?.GetGetMethod(true);
    if (m == null) {
      // try override from base via declared
      m = t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
           .FirstOrDefault(x => x.Name.Contains("CanonicalVars"));
    }
    if (m == null) { Console.WriteLine("no CanonicalVars"); return; }
    DumpCalls(m);
  }

  static void DumpCalls(MethodInfo m) {
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
      } else if (op == 0x14) {
        Console.WriteLine("ldnull");
      }
    }
  }
}
