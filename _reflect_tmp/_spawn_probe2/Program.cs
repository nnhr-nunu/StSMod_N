using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe2", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }

Type T(string name) => types.First(x => x.Name == name || x.FullName == name);

void DumpType(string name)
{
  var t = types.FirstOrDefault(x => x.Name == name || x.FullName == name);
  Console.WriteLine("\n======== " + (t?.FullName ?? ("MISSING "+name)) + " ========");
  if (t == null) return;
  if (t.IsEnum)
  {
    foreach (var v in Enum.GetNames(t)) Console.WriteLine("ENUM " + v + "=" + Convert.ToInt64(Enum.Parse(t, v)));
    return;
  }
  foreach (var c in t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
  {
    var ps = string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine("ctor(" + ps + ")");
  }
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(m => m.Name))
  {
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
    Console.WriteLine((m.IsStatic?"static ":"") + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
  }
  foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(p => p.Name))
    Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name + " get=" + (p.GetMethod!=null) + " set=" + (p.SetMethod!=null));
  foreach (var f in t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(f => f.Name))
    Console.WriteLine("FIELD " + f.FieldType.Name + " " + f.Name);
}

void DumpCalls(Type type, string methodName)
{
  Console.WriteLine("\n----- IL calls in " + type.Name + "." + methodName + " -----");
  foreach (var m in type.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(x => x.Name == methodName || x.Name.Contains(methodName)))
  {
    Console.WriteLine("METHOD " + m);
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo move = m;
    if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
    var body = move.GetMethodBody();
    if (body == null) { Console.WriteLine("  (no body)"); continue; }
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
          if (name.Contains("AsyncTaskMethodBuilder") || name.Contains("TaskAwaiter") || name.Contains("get_IsCompleted") || name.Contains("GetResult") || name.Contains("SetResult") || name.Contains("SetException") || name.Contains("SetStateMachine") || name.Contains("get_Task") || name.Contains("AwaitUnsafe") || name.Contains("AwaitOnCompleted") || name.Contains("ExecutionContext") || name.Contains("ExceptionDispatchInfo") || name.Contains("ConfiguredTask"))
          { i+=4; continue; }
          Console.WriteLine("  " + name + "(" + string.Join(",", member.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
        } catch {}
        i += 4;
      }
      else if (op == 0x72) // ldstr
      {
        var token = BitConverter.ToInt32(il, i + 1);
        try { Console.WriteLine("  ldstr \"" + move.Module.ResolveString(token) + "\""); } catch {}
        i += 4;
      }
    }
  }
}

DumpType("CombatSide");
DumpType("PlayerCmd");
DumpType("OstyCmd");
DumpType("SummonResult");
DumpType("MinionPower");
DumpType("Fabricator");
DumpType("Zapbot");
DumpType("Guardbot");
DumpType("Stabbot");
DumpType("Noisebot");
DumpType("Axebot");
DumpType("BattleFriendV1");
DumpType("BoundPhylactery");
DumpType("VitruvianMinion");
DumpType("Byrdpip");
DumpType("PaelsLegion");

var creatureCmd = T("CreatureCmd");
DumpCalls(creatureCmd, "Add");

var fabricator = T("Fabricator");
DumpCalls(fabricator, "Spawn");
DumpCalls(fabricator, "GenerateMoveStateMachine");

var playerCmd = T("PlayerCmd");
DumpCalls(playerCmd, "AddPet");

var osty = T("OstyCmd");
DumpCalls(osty, "Summon");

Console.WriteLine("\n======== Creature Side/IsPet related methods ========");
var creature = T("Creature");
foreach (var m in creature.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Where(m =>
  m.Name.Contains("Pet") || m.Name.Contains("Side") || m.Name.Contains("Ally") || m.Name.Contains("Enemy") || m.Name.Contains("Primary") || m.Name.Contains("Secondary")))
{
  var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
}

Console.WriteLine("\n======== MonsterModel props about pet/side/minion ========");
var mm = T("MonsterModel");
foreach (var p in mm.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Where(p =>
  p.Name.Contains("Pet") || p.Name.Contains("Minion") || p.Name.Contains("Ally") || p.Name.Contains("Side") || p.Name.Contains("Primary") || p.Name.Contains("Secondary") || p.Name.Contains("Friend")))
  Console.WriteLine("PROP " + p.PropertyType.Name + " " + p.Name);

foreach (var m in mm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly).Where(m =>
  m.Name.Contains("Pet") || m.Name.Contains("Minion") || m.Name.Contains("Summon") || m.Name.Contains("Spawn") || m.Name.Contains("Ally")))
{
  var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
  Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
}

Console.WriteLine("\n======== CombatState Allies/GetTeammates IL? props via reflection of getters ========");
var cs = T("CombatState");
foreach (var name in new[]{"get_Allies","get_Enemies","get_HittableEnemies","get_PlayerCreatures","CreateCreature","AddCreature","GetTeammatesOf","GetOpponentsOf"})
  DumpCalls(cs, name.StartsWith("get_") ? name[4..] : name);

// Also dump get_ methods carefully
foreach (var propName in new[]{"Allies","Enemies","HittableEnemies","PlayerCreatures"})
{
  var prop = cs.GetProperty(propName)!;
  DumpCalls(prop.GetMethod!.DeclaringType!, prop.GetMethod.Name);
}
