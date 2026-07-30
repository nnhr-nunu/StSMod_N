using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var decompiler = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var text = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Commands.PowerCmd"));
var lines = text.Split('\n');
for (int i = 215; i < Math.Min(lines.Length, 310); i++) Console.WriteLine($"L{i}: {lines[i]}");
