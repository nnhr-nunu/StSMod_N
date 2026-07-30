using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var d = new CSharpDecompiler(dll, new DecompilerSettings{ThrowOnAssemblyResolveErrors=false});
foreach (var typeName in new[] {
  "MegaCrit.Sts2.Core.Models.PowerModel",
  "MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext",
  "MegaCrit.Sts2.Core.Commands.Cmd"
})
{
  Console.WriteLine("===== " + typeName + " =====");
  try {
    var t = d.TypeSystem.FindType(new FullTypeName(typeName)).GetDefinition();
    if (t == null) { Console.WriteLine("null"); continue; }
    foreach (var m in t.Methods.Where(m => m.Name is "CustomScaledWait" or "IsVisible" || m.Name.Contains("Visible")))
      Console.WriteLine(d.DecompileAsString(m.MetadataToken) + "\n---");
    foreach (var p in t.Properties.Where(p => p.Name.Contains("Visible") || p.Name == "IsVisible"))
      Console.WriteLine(d.DecompileAsString(p.MetadataToken) + "\n---");
    if (typeName.Contains("Throwing"))
      Console.WriteLine(d.DecompileTypeAsString(new FullTypeName(typeName)));
  } catch (Exception e) { Console.WriteLine(e.Message); }
}
