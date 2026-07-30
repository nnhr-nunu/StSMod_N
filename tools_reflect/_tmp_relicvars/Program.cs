using System.Reflection;

var sts2Dir = args[0];
AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
  var name = new AssemblyName(e.Name).Name + ".dll";
  var p = Path.Combine(sts2Dir, name);
  return File.Exists(p) ? Assembly.LoadFrom(p) : null;
};
var asm = Assembly.LoadFrom(Path.Combine(sts2Dir, "sts2.dll"));
Type[] types;
try { types = asm.GetTypes(); }
catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

void Dump(MethodInfo? m, string label)
{
  if (m?.GetMethodBody() is not {} body) { Console.WriteLine(label+" missing"); return; }
  Console.WriteLine("=== "+label+" ===");
  Console.WriteLine("sig "+m.ReturnType.Name+"("+string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name))+")");
  var il = body.GetILAsByteArray()!;
  var module = m.Module;
  for (var i=0;i<il.Length;i++) {
    var op = il[i];
    if (op is 0x28 or 0x6F or 0x73) {
      try {
        var mem = module.ResolveMethod(BitConverter.ToInt32(il,i+1));
        var tag = op==0x28?"call":op==0x6F?"callvirt":"newobj";
        Console.WriteLine($"  {tag} {mem.DeclaringType?.Name}.{mem.Name}");
      } catch {}
      i+=4;
    } else if (op == 0x7B || op == 0x7E) {
      try { var f=module.ResolveField(BitConverter.ToInt32(il,i+1)); Console.WriteLine((op==0x7B?"  ldfld ":"  ldsfld ")+f.DeclaringType?.Name+"."+f.Name); } catch {}
      i+=4;
    } else if (op==0x72) { try { Console.WriteLine("  ldstr "+module.ResolveString(BitConverter.ToInt32(il,i+1))); } catch {} i+=4; }
    else if (op>=0x16 && op<=0x1E) Console.WriteLine("  ldc.i4."+(op-0x16));
    else if (op==0x1F) { Console.WriteLine("  ldc.i4.s "+unchecked((sbyte)il[i+1])); i++; }
    else if (op==0x02) Console.WriteLine("  ldarg.0");
    else if (op==0x03) Console.WriteLine("  ldarg.1");
    else if (op==0x14) Console.WriteLine("  ldnull");
    else if (op==0x2A) Console.WriteLine("  ret");
    else if (op==0x2C){Console.WriteLine("  brfalse.s"); i++;}
    else if (op==0x2D){Console.WriteLine("  brtrue.s"); i++;}
  }
}

var cv = types.First(t => t.Name=="CalculatedVar");
Dump(cv.GetMethod("Calculate", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance), "CalculatedVar.Calculate");
Dump(cv.GetMethod("UpdateValues", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance), "CalculatedVar.UpdateValues");
Dump(cv.GetMethod("SetOwner", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
  ?? types.First(t=>t.Name=="DynamicVar").GetMethod("SetOwner", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance), "DynamicVar.SetOwner");

var cm = types.First(t => t.Name=="CardModel" && t.Namespace!.EndsWith("Models"));
Dump(cm.GetMethod("UpdateDynamicVarPreview", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly), "CardModel.UpdateDynamicVarPreview");

// MercuryHourglass: find CreatureCmd.Damage in all methods including nested
foreach (var rn in new[]{"MercuryHourglass","Tingsha","LetterOpener","CharonsAshes"})
{
  var t = types.First(x => x.Name==rn);
  Console.WriteLine("## "+rn+" Damage/GainBlock refs (incl nested)");
  foreach (var nt in new[]{t}.Concat(t.GetNestedTypes(BindingFlags.Public|BindingFlags.NonPublic)))
  {
    foreach (var m in nt.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    {
      if (m.GetMethodBody() is not {} body) continue;
      var il = body.GetILAsByteArray()!;
      for (int i=0;i<il.Length-4;i++) if (il[i] is 0x28 or 0x6F) {
        try {
          var mem = m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1));
          if (mem.DeclaringType?.Name=="CreatureCmd" && mem.Name is "Damage" or "GainBlock")
            Console.WriteLine($"  {nt.Name}.{m.Name} -> CreatureCmd.{mem.Name}({string.Join(",", mem.GetParameters().Select(p=>p.ParameterType.Name))})");
          if (mem.Name.Contains("ValueProp") || (mem.DeclaringType?.Name=="ValueProp")) {}
        } catch {}
      }
      // look for ldc.i4 ValueProp near Damage calls - print all ldc near CreatureCmd
    }
  }
}

// Dump MercuryHourglass nested MoveNext for ValueProp constant near Damage
foreach (var rn in new[]{"MercuryHourglass","Orichalcum"})
{
  var t = types.First(x => x.Name==rn);
  foreach (var nt in t.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public))
  {
    var m = nt.GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
    if (m?.GetMethodBody() is not {} body) continue;
    var il = body.GetILAsByteArray()!;
    bool hasDmg=false;
    for (int i=0;i<il.Length-4;i++) if (il[i] is 0x28 or 0x6F) {
      try { var mem=m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1)); if (mem.Name is "Damage" or "GainBlock") hasDmg=true; } catch {}
    }
    if (!hasDmg) continue;
    Console.WriteLine("=== "+rn+"/"+nt.Name+".MoveNext (filtered) ===");
    for (int i=0;i<il.Length;i++) {
      var op=il[i];
      if (op is 0x28 or 0x6F or 0x73) {
        try {
          var mem=m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1));
          if (mem.Name is "Damage" or "GainBlock" or "get_Damage" or "get_Block" or "op_Implicit" || mem.DeclaringType?.Name is "Decimal" or "CreatureCmd" or "DynamicVarSet" or "DynamicVar")
            Console.WriteLine($"  {(op==0x73?"newobj":"call")} {mem.DeclaringType?.Name}.{mem.Name}");
        } catch {}
        i+=4;
      } else if (op>=0x16 && op<=0x1E) Console.WriteLine("  ldc.i4."+(op-0x16));
      else if (op==0x1F){Console.WriteLine("  ldc.i4.s "+unchecked((sbyte)il[i+1])); i++;}
    }
  }
}
