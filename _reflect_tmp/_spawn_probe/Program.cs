using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  if (File.Exists(p)) return c.LoadFromAssemblyPath(p);
  return null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }

void DumpMethods(string typeName)
{
  var t = types.FirstOrDefault(x => x.FullName == typeName) ?? types.FirstOrDefault(x => x.Name == typeName);
  Console.WriteLine("\n======== " + (t?.FullName ?? ("MISSING " + typeName)) + " ========");
  if (t == null) return;
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(m => m.Name))
  {
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine((m.IsStatic?"static ":"") + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
  }
  foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(p => p.Name))
    Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name);
}

string[] names = {
  "CreatureCmd","MonsterCmd","CombatCmd","Creature","CombatState","CombatManager","CombatSide",
  "MinionPower","BattleFriendV1","BattleFriendV2","BattleFriendV3","Fabricator","Zapbot","PunchConstruct",
  "MockAttackAndSummonMinionMonster","PhrogParasite","Ovicopter","Osty","KinFollower","TorchHeadAmalgam"
};
foreach (var n in names) DumpMethods(n);

Console.WriteLine("\n======== TYPES matching spawn/ally/minion/summon/friend ========");
foreach (var t in types.Where(t => t.Name.Contains("Spawn", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Summon", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Minion", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Ally", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Friend", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Companion", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Pet", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Cmd", StringComparison.OrdinalIgnoreCase) && t.Namespace?.Contains("Commands")==true
).OrderBy(t => t.FullName))
  Console.WriteLine(t.FullName);

Console.WriteLine("\n======== Monster methods containing Spawn/Summon/Add ========");
foreach (var t in types.Where(t => t.Name is "MonsterModel" or "Monster" or "Creature" or "CombatState" or "CreatureCmd" or "MonsterCmd" or "EncounterCmd" or "CombatRoom" or "NCombatRoom"))
{
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance).Where(m =>
    m.Name.Contains("Spawn", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("Summon", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("AddCreature", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("CreateCreature", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("AddMonster", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("Ally", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("Friend", StringComparison.OrdinalIgnoreCase)
    || m.Name.Contains("Minion", StringComparison.OrdinalIgnoreCase)
  ).OrderBy(m => m.DeclaringType!.Name + "." + m.Name))
  {
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine(m.DeclaringType!.FullName + "." + m.Name + "(" + ps + ") -> " + m.ReturnType.Name);
  }
}

Console.WriteLine("\n======== Types with Zap/Machine/Fabricator ========");
foreach (var t in types.Where(t => t.Name.Contains("Zap", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Fabricator", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("Machine", StringComparison.OrdinalIgnoreCase)
  || t.Name.Contains("BattleFriend", StringComparison.OrdinalIgnoreCase)
).OrderBy(t => t.FullName))
  Console.WriteLine(t.FullName + " : " + t.BaseType?.Name);
