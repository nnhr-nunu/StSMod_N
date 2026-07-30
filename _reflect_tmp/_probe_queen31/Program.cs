using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
var nested = cm.GetNestedTypes(BindingFlags.NonPublic).First(t => t.Name == "<>c");

foreach (var f in nested.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
{
    if (f.Name.Contains("99")) Console.WriteLine($"Field {f.Name} type={f.FieldType}");
}

foreach (var t in cm.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (m.Name.Contains("99")) Console.WriteLine($"Method {t.Name}.{m.Name}");
    }
}

// Manual parse IsEnding logic from IL bytes
// After Any: if Any true -> not ending? if false -> check ShouldStopCombatFromEnding
var getIsEnding = cm.GetMethod("get_IsEnding")!;
var il = getIsEnding.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nIsEnding IL disassembly (simplified):");
for (var i = 0; i < il.Length; i++)
{
    var op = il[i];
  string line = $"{i:X4}: {op:X2}";
  if (op is 0x16 or 0x17 or 0x18) line += " const";
  if (op is 0x2A) line += " ret";
  if (op is 0x2C or 0x2D) line += " branch";
  if (op is 0x28 or 0x6F)
  {
      try { line += " -> " + getIsEnding.Module.ResolveMember(BitConverter.ToInt32(il, i+1)); } catch {}
      i += 4;
  }
  Console.WriteLine(line);
}
