using System;
using System.Linq;
using Mono.Cecil;
var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\HypnosisCreator\HypnosisCreator.dll";
var asm = AssemblyDefinition.ReadAssembly(path);
var t = asm.MainModule.GetTypes().First(x => x.Name == "AmbushHypnosis");
var ctor = t.Methods.First(m => m.IsConstructor && !m.IsStatic);
Console.WriteLine("ctors: " + string.Join(" | ", t.Methods.Where(m => m.Name.Contains("ctor") || m.Name == ".ctor").Select(m => m.Name)));
// base call energy - look at cctor or instance ctor body for ldc.i4.1
foreach (var m in t.Methods.Where(m => m.HasBody))
{
  if (m.Name is ".ctor" or "OnUpgrade" or "CalcDraw" or "get_CanonicalVars" or "OnPlay")
  {
    Console.WriteLine("--- " + m.Name + " ---");
    foreach (var i in m.Body.Instructions.Take(25)) Console.WriteLine(i);
  }
}
