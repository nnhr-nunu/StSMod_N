using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

var asm = AssemblyDefinition.ReadAssembly(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
void DumpTypeMethods(string typeName, params string[] methods) {
  var t = asm.MainModule.Types.First(x => x.FullName == typeName || x.Name == typeName);
  foreach (var nested in t.NestedTypes) {
    if (!nested.Name.Contains("ZapMove") && !nested.Name.Contains("AfterAdded")) continue;
    Console.WriteLine("NESTED " + nested.FullName);
    var move = nested.Methods.FirstOrDefault(m => m.Name == "MoveNext");
    if (move == null) continue;
    foreach (var i in move.Body.Instructions) {
      if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt || i.OpCode == OpCodes.Newobj || i.OpCode == OpCodes.Ldstr || i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Stfld)
        Console.WriteLine("  " + i);
    }
  }
}
DumpTypeMethods("MegaCrit.Sts2.Core.Models.Monsters.Zapbot");

// Also PlayerCmd.AddPet generic
var pc = asm.MainModule.Types.First(x => x.Name == "PlayerCmd");
foreach (var m in pc.Methods.Where(m => m.Name == "AddPet")) {
  Console.WriteLine("AddPet " + m.FullName);
  foreach (var i in m.Body.Instructions.Take(80))
    if (i.OpCode.Name.StartsWith("call") || i.OpCode == OpCodes.Newobj || i.OpCode == OpCodes.Ldstr)
      Console.WriteLine("  " + i);
}

// AttackCommand.FromMonster
var ac = asm.MainModule.Types.SelectMany(t => t.NestedTypes.Append(t)).First(x => x.Name == "AttackCommand" && x.Namespace.Contains("Builders"));
var fm = ac.Methods.First(m => m.Name == "FromMonster");
Console.WriteLine("FromMonster:");
foreach (var i in fm.Body.Instructions) Console.WriteLine("  " + i);

// How DamageCmd.Attack sets attacker - look at card Kick pattern via search for FromMonster usages count
var uses = 0;
foreach (var t in asm.MainModule.Types) {
  foreach (var m in t.Methods) {
    if (m.Body == null) continue;
    foreach (var i in m.Body.Instructions) {
      if (i.Operand is MethodReference mr && mr.Name == "FromMonster") {
        Console.WriteLine("USE FromMonster in " + t.Name + "." + m.Name);
        if (++uses > 15) goto done;
      }
    }
  }
}
done:;
