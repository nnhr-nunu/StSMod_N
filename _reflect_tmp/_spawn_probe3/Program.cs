using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe3", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
Type T(string name) => types.First(x => x.Name == name || x.FullName == name);

void DumpCalls(MethodBase move, string label)
{
  Console.WriteLine("\n----- " + label + " -----");
  var body = move.GetMethodBody();
  if (body == null) { Console.WriteLine("(no body)"); return; }
  var il = body.GetILAsByteArray()!;
  for (var i = 0; i < il.Length; i++)
  {
    var op = il[i];
    if (op is 0x28 or 0x6F or 0x73)
    {
      var token = BitConverter.ToInt32(il, i + 1);
      try {
        var member = move.Module.ResolveMethod(token);
        var name = member!.DeclaringType?.Name + "." + member.Name;
        if (name.Contains("AsyncTaskMethodBuilder") || name.Contains("TaskAwaiter") || name.Contains("get_IsCompleted") || name.Contains("GetResult") || name.Contains("SetResult") || name.Contains("SetException") || name.Contains("SetStateMachine") || name.Contains("get_Task") || name.Contains("AwaitUnsafe") || name.Contains("AwaitOnCompleted") || name.Contains("ExecutionContext") || name.Contains("ExceptionDispatchInfo") || name.Contains("ConfiguredTaskAwaitable") || name.Contains("ConfiguredTaskAwaiter"))
        { i+=4; continue; }
        var ps = string.Join(",", member.GetParameters().Select(p=>p.ParameterType.Name));
        Console.WriteLine("  " + (op==0x73?"new ":"") + name + "(" + ps + ")");
      } catch {}
      i += 4;
    }
    else if (op == 0x72)
    {
      var token = BitConverter.ToInt32(il, i + 1);
      try { Console.WriteLine("  ldstr \"" + move.Module.ResolveString(token) + "\""); } catch {}
      i += 4;
    }
    else if (op == 0x74 || op == 0x75) // ldtoken / something - skip
    {
      i += 4;
    }
  }
}

MethodInfo ResolveMoveNext(MethodInfo m)
{
  var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
  if (attr == null) return m;
  return ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
}

void DumpMethod(Type type, string name)
{
  foreach (var m in type.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(x => x.Name == name))
  {
    Console.WriteLine("\n### " + type.Name + "." + m.Name + " :: " + m);
    DumpCalls(ResolveMoveNext(m), type.Name + "." + m.Name);
  }
}

void DumpProp(Type type, string prop)
{
  var p = type.GetProperty(prop, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);
  if (p == null) { Console.WriteLine("MISSING PROP " + type.Name + "." + prop); return; }
  if (p.GetMethod != null) DumpCalls(p.GetMethod, type.Name + ".get_" + prop);
}

var creatureCmd = T("CreatureCmd");
DumpMethod(creatureCmd, "Add");

var fabricator = T("Fabricator");
DumpMethod(fabricator, "SpawnBot");
DumpMethod(fabricator, "SpawnAggroBot");
DumpMethod(fabricator, "SpawnDefensiveBot");
DumpMethod(fabricator, "FabricateMove");
DumpProp(fabricator, "CanFabricate");

var creature = T("Creature");
DumpProp(creature, "IsPet");
DumpProp(creature, "IsEnemy");
DumpProp(creature, "IsPrimaryEnemy");
DumpProp(creature, "IsSecondaryEnemy");
DumpProp(creature, "IsPlayer");
DumpProp(creature, "IsMonster");
DumpProp(creature, "IsHittable");

var cs = T("CombatState");
DumpProp(cs, "Allies");
DumpProp(cs, "Enemies");

var bound = T("BoundPhylactery");
DumpMethod(bound, "SummonPet");
DumpMethod(bound, "BeforeCombatStart");

var byrd = T("Byrdpip");
DumpMethod(byrd, "SummonPet");

var mock = types.First(x => x.Name == "MockAttackAndSummonMinionMonster");
DumpMethod(mock, "AttackAndSummonMinionMove");

var zap = T("Zapbot");
DumpMethod(zap, "ZapMove");
DumpMethod(zap, "AfterAddedToRoom");
DumpProp(zap, "ZapDamage");
DumpProp(zap, "MinInitialHp");
DumpProp(zap, "MaxInitialHp");

var bf = T("BattleFriendV1");
DumpMethod(bf, "AfterAddedToRoom");
DumpMethod(bf, "GenerateMoveStateMachine");

// ModelDb.Monster usage patterns via MinionDiveBomb?
Console.WriteLine("\n======== Cards that summon ========");
foreach (var t in types.Where(t => t.Namespace?.Contains("Cards")==true && (
  t.Name.Contains("Minion") || t.Name.Contains("Summon") || t.Name.Contains("Friend") || t.Name.Contains("Osty") || t.Name.Contains("Pet"))))
  Console.WriteLine(t.FullName);

foreach (var cardName in new[]{"MinionStrike","MinionDiveBomb","MinionSacrifice","Friendship","Rally"})
{
  var t = types.FirstOrDefault(x => x.Name == cardName);
  if (t == null) continue;
  Console.WriteLine("\n==== CARD " + cardName + " methods ====");
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
  {
    var ps = string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name));
    Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
  }
}

// PlayerCombatState AddPetInternal
var pcs = T("PlayerCombatState");
Console.WriteLine("\n======== PlayerCombatState pet APIs ========");
foreach (var m in pcs.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m => m.Name.Contains("Pet") || m.Name.Contains("Osty")))
{
  var ps = string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
}
foreach (var p in pcs.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(p => p.Name.Contains("Pet") || p.Name.Contains("Osty")))
  Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name);

DumpMethod(pcs, "AddPetInternal");

// Check slot names used by Fabricator / encounter
Console.WriteLine("\n======== Encounter FabricatorNormal slots ========");
var enc = T("FabricatorNormal");
foreach (var m in enc.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
{
  var ps = string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
}
foreach (var p in enc.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
  Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name);
DumpProp(enc, "Slots");
DumpMethod(enc, "GenerateMonsters");
