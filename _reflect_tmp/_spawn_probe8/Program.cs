using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe8", true);
alc.Resolving += (c, n) => { var p = Path.Combine(dir, n.Name + ".dll"); return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null; };
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types; try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
Type T(string n) => types.First(x => x.Name == n);
void D(MethodBase m, string label) {
  Console.WriteLine("\n== "+label+" ==");
  var il = m.GetMethodBody()!.GetILAsByteArray()!;
  for (var i=0;i<il.Length;i++) {
    var op=il[i];
    if (op>=0x16&&op<=0x1E){Console.WriteLine("ldc.i4."+(op-0x16));continue;}
    if (op==0x1F){Console.WriteLine("ldc.i4.s "+(sbyte)il[i+1]);i++;continue;}
    if (op==0x20){Console.WriteLine("ldc.i4 "+BitConverter.ToInt32(il,i+1));i+=4;continue;}
    if (op==0x72){try{Console.WriteLine("ldstr "+m.Module.ResolveString(BitConverter.ToInt32(il,i+1)));}catch{}i+=4;continue;}
    if (op is 0x28 or 0x6F or 0x73){try{var mm=m.Module.ResolveMethod(BitConverter.ToInt32(il,i+1)); Console.WriteLine(mm.DeclaringType?.Name+"."+mm.Name);}catch{}i+=4;continue;}
  }
}
D(T("Zapbot").GetProperty("ZapDamage")!.GetMethod!, "ZapDamage");
D(T("Zapbot").GetProperty("MinInitialHp")!.GetMethod!, "MinHp");
D(T("Zapbot").GetProperty("MaxInitialHp")!.GetMethod!, "MaxHp");
D(T("Creature").GetProperty("IsSecondaryEnemy")!.GetMethod!, "IsSecondaryEnemy");
Console.WriteLine("AttackCommand.FromMonster overloads:");
foreach (var m in T("AttackCommand").GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance).Where(x=>x.Name.Contains("FromMonster")||x.Name=="FromMonster")) Console.WriteLine(m);
var fm = typeof(Enumerable).Assembly; // noop
var ac = T("AttackCommand");
foreach (var m in ac.GetMethods(BindingFlags.Public|BindingFlags.Static)) if (m.Name=="FromMonster") Console.WriteLine("static "+m);
// Find FromMonster via declared
foreach (var m in ac.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly))
  if (m.Name.Contains("Monster")||m.Name.Contains("Osty")) Console.WriteLine(m);
