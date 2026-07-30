using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe5", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
Type T(string name) => types.First(x => x.Name == name || x.FullName == name);

void DumpAllLdtoken(MethodBase move, string label)
{
  Console.WriteLine("\n===== " + label + " =====");
  var body = move.GetMethodBody(); if (body==null) return;
  var il = body.GetILAsByteArray()!;
  for (var i = 0; i < il.Length; i++)
  {
    var op = il[i];
    if (op == 0xD0)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try { Console.WriteLine("ldtoken TYPE " + move.Module.ResolveType(token).FullName); }
      catch {
        try { Console.WriteLine("ldtoken METHOD " + move.Module.ResolveMethod(token)); }
        catch {
          try { Console.WriteLine("ldtoken FIELD " + move.Module.ResolveField(token)); } catch {}
        }
      }
      i += 4;
    }
    else if (op == 0x72)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try { Console.WriteLine("ldstr \"" + move.Module.ResolveString(token) + "\""); } catch {}
      i += 4;
    }
    else if (op is 0x28 or 0x6F or 0x73)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try {
        var m = move.Module.ResolveMethod(token);
        if (m is MethodInfo mi && mi.IsGenericMethod)
          Console.WriteLine("CALL " + mi.DeclaringType?.Name + "." + mi.Name + "<" + string.Join(",", mi.GetGenericArguments().Select(g=>g.FullName ?? g.Name)) + ">");
        else
          Console.WriteLine("CALL " + m.DeclaringType?.Name + "." + m.Name);
      } catch {}
      i += 4;
    }
  }
}

var fab = T("Fabricator");
DumpAllLdtoken(fab.TypeInitializer!, "Fabricator.cctor");

// CreatureCmd.Add[T] - which CombatSide?
var addT = T("CreatureCmd").GetMethods().First(m => m.Name=="Add" && m.IsGenericMethodDefinition);
DumpAllLdtoken(addT.GetCustomAttributesData().First(x=>x.AttributeType.Name=="AsyncStateMachineAttribute") is var attr
  ? ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!
  : addT, "CreatureCmd.Add[T] MoveNext");

// Also check non-async wrapper
DumpAllLdtoken(addT, "CreatureCmd.Add[T] wrapper");

// PlayerCmd.AddPet[T] side
var addPetT = T("PlayerCmd").GetMethods().First(m => m.Name=="AddPet" && m.IsGenericMethodDefinition);
var petAttr = addPetT.GetCustomAttributesData().FirstOrDefault(x=>x.AttributeType.Name=="AsyncStateMachineAttribute");
if (petAttr != null)
  DumpAllLdtoken(((Type)petAttr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!, "PlayerCmd.AddPet[T]");
else
  DumpAllLdtoken(addPetT, "PlayerCmd.AddPet[T] sync wrapper");

// CombatManager StartTurn / ExecuteEnemyTurn deeper
Console.WriteLine("\n===== CombatManager methods involving TakeTurn =====");
var cm = T("CombatManager");
foreach (var m in cm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
{
  if (!(m.Name.Contains("Turn") || m.Name.Contains("Enemy") || m.Name.Contains("Execute"))) continue;
  Console.WriteLine(m.Name + " -> " + m.ReturnType.Name);
}

void DumpTakeTurnFlow(string methodName)
{
  var m = cm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).FirstOrDefault(x => x.Name == methodName);
  if (m == null) { Console.WriteLine("missing "+methodName); return; }
  Console.WriteLine("\n##### " + methodName + " #####");
  var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
  MethodInfo move = m;
  if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
  var body = move.GetMethodBody(); if (body==null) return;
  var il = body.GetILAsByteArray()!;
  for (var i=0;i<il.Length;i++)
  {
    if (il[i] is 0x28 or 0x6F or 0x73)
    {
      var token=BitConverter.ToInt32(il,i+1);
      try {
        var mem=move.Module.ResolveMethod(token);
        var n = mem!.DeclaringType?.Name + "." + mem.Name;
        if (n.Contains("Async")||n.Contains("Await")||n.Contains("Task")||n.Contains("ExecutionContext")||n.Contains("ExceptionDispatch")||n.Contains("Configured")||n.Contains("IsCompleted")||n.Contains("GetResult")||n.Contains("SetResult")||n.Contains("SetException")||n.Contains("SetStateMachine")||n.Contains("get_Task")) { i+=4; continue; }
        Console.WriteLine("  " + n);
      } catch {}
      i+=4;
    }
    else if (il[i]==0x72)
    {
      var token=BitConverter.ToInt32(il,i+1);
      try { Console.WriteLine("  ldstr \""+move.Module.ResolveString(token)+"\""); } catch {}
      i+=4;
    }
  }
}

DumpTakeTurnFlow("StartTurn");
DumpTakeTurnFlow("ExecuteEnemyTurn");

// Creature.TakeTurn
DumpAllLdtoken(
  ((Type)T("Creature").GetMethod("TakeTurn")!.GetCustomAttributesData().First(x=>x.AttributeType.Name=="AsyncStateMachineAttribute").ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!,
  "Creature.TakeTurn");

// AttackCommand targets - FromMonster how picks targets
Console.WriteLine("\n===== AttackCommand methods =====");
var ac = T("AttackCommand");
foreach (var m in ac.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
{
  var ps=string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "("+ps+")");
}

// GetOpponentsOf used by monster moves?
Console.WriteLine("\n===== MonsterModel PerformMove / GetTargets =====");
var mm = T("MonsterModel");
foreach (var m in mm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m =>
  m.Name.Contains("Target") || m.Name.Contains("Perform") || m.Name.Contains("TakeTurn") || m.Name.Contains("Opponent") || m.Name.Contains("RollMove")))
{
  var ps=string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "("+ps+")");
}

// HighVoltagePower
Console.WriteLine("\n===== HighVoltagePower =====");
var hv = types.FirstOrDefault(t => t.Name == "HighVoltagePower");
if (hv != null)
{
  foreach (var p in hv.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
    Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name);
  foreach (var m in hv.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
    Console.WriteLine(m.ReturnType.Name + " " + m.Name);
}

// Loc from monsters.json alternate paths
foreach (var root in new[]{
  @"D:\Dev\antigravity\StSMod_N\_vanilla_loc_extract\sts2\localization\eng",
  @"D:\Dev\antigravity\StSMod_N\_vanilla_loc_extract\sts2\localization\jpn"})
{
  if (!Directory.Exists(root)) continue;
  foreach (var f in Directory.GetFiles(root, "*.json"))
  {
    foreach (var line in File.ReadLines(f))
    {
      if (line.Contains("ZAPBOT", StringComparison.OrdinalIgnoreCase) ||
          line.Contains("FABRICATOR", StringComparison.OrdinalIgnoreCase) ||
          (line.Contains("Zap") && line.Contains("bot", StringComparison.OrdinalIgnoreCase)) ||
          line.Contains("ザップ") || line.Contains("ファブリケーター"))
        Console.WriteLine(Path.GetFileName(f)+": "+line.Trim());
    }
  }
}

// Check NCombatRoom.AddCreature for side positioning
Console.WriteLine("\n===== NCombatRoom.AddCreature =====");
var ncr = T("NCombatRoom");
var addC = ncr.GetMethod("AddCreature", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
if (addC != null) DumpAllLdtoken(addC, "NCombatRoom.AddCreature");
