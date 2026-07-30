using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe6", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
Type T(string name) => types.First(x => x.Name == name || x.FullName == name);

MethodInfo MN(MethodInfo m)
{
  var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
  if (attr == null) return m;
  return ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
}

void Dump(MethodBase move, string label)
{
  Console.WriteLine("\n===== " + label + " =====");
  var body = move.GetMethodBody(); if (body == null) return;
  var il = body.GetILAsByteArray()!;
  for (var i = 0; i < il.Length; i++)
  {
    var op = il[i];
    if (op >= 0x16 && op <= 0x1E) { Console.WriteLine($"{i}: ldc.i4.{op-0x16}"); continue; }
    if (op == 0x15) { Console.WriteLine($"{i}: ldc.i4.m1"); continue; }
    if (op == 0x1F) { Console.WriteLine($"{i}: ldc.i4.s {(sbyte)il[i+1]}"); i++; continue; }
    if (op == 0x20) { Console.WriteLine($"{i}: ldc.i4 {BitConverter.ToInt32(il,i+1)}"); i+=4; continue; }
    if (op == 0x14) { Console.WriteLine($"{i}: ldnull"); continue; }
    if (op == 0x72) { var t=BitConverter.ToInt32(il,i+1); try{Console.WriteLine($"{i}: ldstr \"{move.Module.ResolveString(t)}\"");}catch{} i+=4; continue; }
    if (op is 0x28 or 0x6F or 0x73)
    {
      var t=BitConverter.ToInt32(il,i+1);
      try {
        var m=move.Module.ResolveMethod(t);
        var g="";
        if (m is MethodInfo mi && mi.IsGenericMethod) g="<"+string.Join(",", mi.GetGenericArguments().Select(x=>x.Name))+">";
        Console.WriteLine($"{i}: call {m.DeclaringType?.Name}.{m.Name}{g}");
      } catch {}
      i+=4; continue;
    }
    if (op is 0x7B or 0x7D)
    {
      var t=BitConverter.ToInt32(il,i+1);
      try { var f=move.Module.ResolveField(t); Console.WriteLine($"{i}: {(op==0x7B?"ldfld":"stfld")} {f.DeclaringType?.Name}.{f.Name}"); } catch {}
      i+=4; continue;
    }
  }
}

var addT = T("CreatureCmd").GetMethods().First(m => m.Name=="Add" && m.IsGenericMethodDefinition);
Dump(MN(addT), "CreatureCmd.Add[T]");

var addSide = T("CreatureCmd").GetMethods().First(m => m.Name=="Add" && !m.IsGenericMethod && m.GetParameters().Length==4);
Dump(MN(addSide), "CreatureCmd.Add(side)");

Dump(T("AttackCommand").GetMethod("FromMonster", BindingFlags.Public|BindingFlags.Static)!, "FromMonster");
Dump(T("AttackCommand").GetMethod("TargetingAllOpponents", BindingFlags.Public|BindingFlags.Instance)!, "TargetingAllOpponents");
Dump(T("AttackCommand").GetMethod("GetPossibleTargets", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!, "GetPossibleTargets");
Dump(MN(T("Creature").GetMethod("TakeTurn")!), "TakeTurn");

foreach (var prop in new[]{"Allies","Enemies"})
{
  var g = T("CombatState").GetProperty(prop)!.GetMethod!;
  Console.WriteLine($"\n{prop} IL len={g.GetMethodBody()?.GetILAsByteArray()?.Length}");
  Dump(g, "get_"+prop);
}

Dump(MN(T("Friendship").GetMethod("OnPlay", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!), "Friendship.OnPlay");
Dump(MN(T("MinionStrike").GetMethod("OnPlay", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!), "MinionStrike.OnPlay");

// Zap damage values via const fields? Use AscensionHelper args from IL
Dump(T("Zapbot").GetProperty("ZapDamage")!.GetMethod!, "ZapDamage");
Dump(T("Zapbot").GetProperty("MinInitialHp")!.GetMethod!, "ZapMinHp");
Dump(T("Zapbot").GetProperty("MaxInitialHp")!.GetMethod!, "ZapMaxHp");
