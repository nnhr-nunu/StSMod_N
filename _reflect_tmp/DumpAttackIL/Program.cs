using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
class P {
static void Main() {
var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var d = new CSharpDecompiler(path, new DecompilerSettings{ThrowOnAssemblyResolveErrors=false});
var t = d.TypeSystem.FindType(new FullTypeName("MegaCrit.Sts2.Core.Commands.Cmd")).GetDefinition()!;
foreach (var m in t.Methods.Where(m => m.Name == "Wait"))
  Console.WriteLine(d.DecompileAsString(m.MetadataToken) + "\n---");
}
}
