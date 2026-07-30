using Mono.Cecil;
using Mono.Cecil.Cil;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = AssemblyDefinition.ReadAssembly(path);
var t = asm.MainModule.Types.First(x => x.Name == "LocalContext");
Console.WriteLine(t.FullName);
foreach (var m in t.Methods) Console.WriteLine(m.FullName);
foreach (var p in t.Properties) Console.WriteLine("P " + p.Name);
foreach (var f in t.Fields) Console.WriteLine("F " + f.Name);
