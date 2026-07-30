using Mono.Cecil;
using Mono.Cecil.Cil;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var module = ModuleDefinition.ReadModule(dll);

// FeelNoPain full IL for GainBlock args
var fnp = module.Types.First(x => x.Name == "FeelNoPainPower");
var nested = fnp.NestedTypes.First(n => n.Name.Contains("AfterCardExhausted"));
var mn = nested.Methods.First(m => m.Name == "MoveNext");
Console.WriteLine("=== FeelNoPain AfterCardExhausted full ===");
foreach (var i in mn.Body.Instructions)
    Console.WriteLine($"  IL_{i.Offset:X4}: {i}");

// ValueProp constants
var vp = module.Types.First(t => t.Name == "ValueProp");
Console.WriteLine("\nValueProp flags:");
foreach (var f in vp.Fields.Where(f => f.HasConstant))
    Console.WriteLine($"  {f.Name} = {f.Constant}");

// Feel No Pain card CanonicalVars
var fnpc = module.Types.First(x => x.Name == "FeelNoPain");
var cv = fnpc.Methods.First(m => m.Name == "get_CanonicalVars");
Console.WriteLine("\n=== FeelNoPain CanonicalVars ===");
foreach (var i in cv.Body.Instructions)
    Console.WriteLine($"  {i}");

// FlameBarrier card
var fbc = module.Types.First(x => x.Name == "FlameBarrier");
Console.WriteLine("\n=== FlameBarrier CanonicalVars ===");
foreach (var i in fbc.Methods.First(m => m.Name == "get_CanonicalVars").Body.Instructions)
    Console.WriteLine($"  {i}");
