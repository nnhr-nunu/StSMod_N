using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Collections;

var sts2Path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var dir = Path.GetDirectoryName(sts2Path)!;
var alc = new AssemblyLoadContext("probe4", true);
alc.Resolving += (c, n) => {
  var p = Path.Combine(dir, n.Name + ".dll");
  return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null;
};
var a = alc.LoadFromAssemblyPath(sts2Path);
Type[] types;
try { types = a.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
Type T(string name) => types.First(x => x.Name == name || x.FullName == name);

// Resolve generic method tokens in SpawnBot / Zapbot.AfterAddedToRoom / BattleFriend.AfterAddedToRoom / CreatureCmd.Add[T]
void DumpGenericCalls(Type type, string methodName)
{
  Console.WriteLine("\n===== GENERIC/TOKENS " + type.Name + "." + methodName + " =====");
  foreach (var m in type.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(x => x.Name == methodName))
  {
    Console.WriteLine("METHOD " + m);
    if (m.IsGenericMethodDefinition)
      Console.WriteLine("  generic def params: " + string.Join(",", m.GetGenericArguments().Select(g=>g.Name)));
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo move = m;
    if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
    var body = move.GetMethodBody();
    if (body == null) continue;
    var il = body.GetILAsByteArray()!;
    for (var i = 0; i < il.Length; i++)
    {
      var op = il[i];
      if (op == 0x28 || op == 0x6F || op == 0x73)
      {
        var token = BitConverter.ToInt32(il, i+1);
        try {
          var member = move.Module.ResolveMethod(token);
          if (member is MethodInfo mi && mi.IsGenericMethod)
            Console.WriteLine("  GENERIC CALL " + mi.DeclaringType?.Name + "." + mi.Name + "<" + string.Join(",", mi.GetGenericArguments().Select(g=>g.Name)) + ">(" + string.Join(",", mi.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
          else if (member.Name.Contains("Apply") || member.Name.Contains("Add") || member.Name.Contains("Monster") || member.Name.Contains("Power") || member.Name.Contains("CreateCreature"))
            Console.WriteLine("  CALL " + member.DeclaringType?.Name + "." + member.Name + "(" + string.Join(",", member.GetParameters().Select(p=>p.ParameterType.Name)) + ") ret=" + ((MethodInfo)member).ReturnType.Name);
        } catch {}
        i += 4;
      }
      else if (op == 0xD0) // ldtoken
      {
        var token = BitConverter.ToInt32(il, i+1);
        try {
          var ht = move.Module.ResolveType(token);
          Console.WriteLine("  ldtoken TYPE " + ht.FullName);
        } catch {
          try {
            var hm = move.Module.ResolveMethod(token);
            Console.WriteLine("  ldtoken METHOD " + hm);
          } catch {
            try {
              var hf = move.Module.ResolveField(token);
              Console.WriteLine("  ldtoken FIELD " + hf);
            } catch {}
          }
        }
        i += 4;
      }
    }
  }
}

DumpGenericCalls(T("Fabricator"), "SpawnBot");
DumpGenericCalls(T("Zapbot"), "AfterAddedToRoom");
DumpGenericCalls(T("BattleFriendV1"), "AfterAddedToRoom");
DumpGenericCalls(T("MockAttackAndSummonMinionMonster"), "AttackAndSummonMinionMove");
DumpGenericCalls(T("CreatureCmd"), "Add");
DumpGenericCalls(T("PlayerCmd"), "AddPet");
DumpGenericCalls(T("Byrdpip"), "SummonPet");

// Instantiate Fabricator via formatter services? Better: read static ctor / field initializers via IL of .ctor and field rva
Console.WriteLine("\n===== Fabricator field initializers / ctor =====");
var fab = T("Fabricator");
foreach (var c in fab.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static))
{
  Console.WriteLine("CTOR " + c);
  var body = c.GetMethodBody();
  if (body == null) continue;
  var il = body.GetILAsByteArray()!;
  for (var i = 0; i < il.Length; i++)
  {
    var op = il[i];
    if (op == 0x72)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try { Console.WriteLine("  ldstr \"" + c.Module.ResolveString(token) + "\""); } catch {}
      i += 4;
    }
    else if (op is 0x28 or 0x6F or 0x73)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try {
        var member = c.Module.ResolveMethod(token);
        Console.WriteLine("  " + member.DeclaringType?.Name + "." + member.Name);
      } catch {}
      i += 4;
    }
    else if (op == 0xD0)
    {
      var token = BitConverter.ToInt32(il, i+1);
      try { Console.WriteLine("  ldtoken " + c.Module.ResolveType(token).FullName); } catch {}
      i += 4;
    }
  }
}

// Try cctor of Fabricator
var cctor = fab.GetConstructor(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public, null, Type.EmptyTypes, null)
  ?? fab.TypeInitializer;
if (cctor != null)
{
  Console.WriteLine("\nCCTOR found");
  var body = cctor.GetMethodBody()!;
  var il = body.GetILAsByteArray()!;
  for (var i = 0; i < il.Length; i++)
  {
    var op = il[i];
    if (op == 0x72) { var token = BitConverter.ToInt32(il,i+1); try{Console.WriteLine(" ldstr "+cctor.Module.ResolveString(token));}catch{} i+=4;}
    else if (op == 0xD0) { var token=BitConverter.ToInt32(il,i+1); try{Console.WriteLine(" ldtoken "+cctor.Module.ResolveType(token).FullName);}catch{} i+=4;}
    else if (op is 0x28 or 0x6F or 0x73) { var token=BitConverter.ToInt32(il,i+1); try{var m=cctor.Module.ResolveMethod(token); Console.WriteLine(" "+m.DeclaringType?.Name+"."+m.Name);}catch{} i+=4;}
  }
}

// Read aggroSpawns / defenseSpawns via creating instance without full game? Might fail.
// Instead decompile field init from ctor IL of field initializers - often in ctor.

// PowerCmd.Apply signatures
Console.WriteLine("\n===== PowerCmd.Apply overloads =====");
foreach (var m in T("PowerCmd").GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m => m.Name == "Apply"))
  Console.WriteLine(m);

// CombatManager StartTurn creature selection for player side with pets
Console.WriteLine("\n===== CombatManager enemy/player turn creature iteration keywords =====");
var cm = T("CombatManager");
foreach (var m in cm.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m => m.Name is "StartTurn" or "TakeEnemyTurn" or "RunEnemyTurn" or "DoEnemyTurn" or "EnemyTurn" or "AfterCreatureAdded"))
{
  Console.WriteLine(m.Name);
  var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
  MethodInfo move = m;
  if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
  var body = move.GetMethodBody(); if (body==null) continue;
  var il = body.GetILAsByteArray()!;
  for (var i=0;i<il.Length;i++)
  {
    if (il[i] is 0x28 or 0x6F)
    {
      var token=BitConverter.ToInt32(il,i+1);
      try {
        var mem=move.Module.ResolveMethod(token);
        var n=mem!.DeclaringType?.Name+"."+mem.Name;
        if (n.Contains("Creatures") || n.Contains("Enemy") || n.Contains("Ally") || n.Contains("Pet") || n.Contains("TakeTurn") || n.Contains("CurrentSide") || n.Contains("GetCreatures") || n.Contains("IsAlive") || n.Contains("IsMonster") || n.Contains("IsPlayer") || n.Contains("IsPet") || n.Contains("IsSecondary"))
          Console.WriteLine("  " + n);
      } catch {}
      i+=4;
    }
  }
}

// Loc strings for monsters
Console.WriteLine("\n===== Loc entries =====");
foreach (var locPath in Directory.GetFiles(@"D:\Dev\antigravity\StSMod_N\_vanilla_loc_extract\sts2\localization\eng", "*monster*"))
  Console.WriteLine("file "+locPath);
foreach (var locPath in Directory.GetFiles(@"D:\Dev\antigravity\StSMod_N\_vanilla_loc_extract\sts2\localization\eng", "*.json"))
{
  var text = File.ReadAllText(locPath);
  if (text.Contains("ZAPBOT") || text.Contains("FABRICATOR") || text.Contains("BATTLE_FRIEND") || text.Contains("GUARDBOT") || text.Contains("STABBOT"))
  {
    foreach (var line in File.ReadLines(locPath))
      if (line.Contains("ZAPBOT") || line.Contains("FABRICATOR") || line.Contains("BATTLE_FRIEND") || line.Contains("GUARDBOT") || line.Contains("STABBOT") || line.Contains("NOISEBOT") || line.Contains("AXEBOT"))
        Console.WriteLine(Path.GetFileName(locPath)+": "+line.Trim());
  }
}

// BaseLib pet/spawn?
var bl = Assembly.LoadFrom(@"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.6\lib\net9.0\BaseLib.dll");
Type[] bt; try { bt = bl.GetTypes(); } catch (ReflectionTypeLoadException ex) { bt = ex.Types.Where(t=>t!=null).Cast<Type>().ToArray(); }
Console.WriteLine("\n===== BaseLib spawn/pet/ally =====");
foreach (var t in bt.Where(t => t.Name.Contains("Spawn") || t.Name.Contains("Pet") || t.Name.Contains("Ally") || t.Name.Contains("Summon") || t.Name.Contains("Minion") || t.Name.Contains("Creature")))
  Console.WriteLine(t.FullName);

// ModelDb.Monster generic
Console.WriteLine("\n===== ModelDb Monster methods =====");
foreach (var m in T("ModelDb").GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m => m.Name.Contains("Monster") || m.Name.Contains("Pet")))
  Console.WriteLine(m);

// EncounterModel.GetNextSlot
DumpGenericCalls(T("EncounterModel"), "GetNextSlot");
Console.WriteLine("\nGetNextSlot methods:");
foreach (var m in T("EncounterModel").GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Where(m => m.Name.Contains("Slot")))
  Console.WriteLine(m);
