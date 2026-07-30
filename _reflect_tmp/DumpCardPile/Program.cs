using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var d = new CSharpDecompiler(dll, new DecompilerSettings());
var t = d.TypeSystem.FindType(new FullTypeName("MegaCrit.Sts2.Core.Commands.CardPileCmd")).GetDefinition();
var m = t.Methods.First(x => x.Name == "Add" && x.Parameters.Count == 5 && x.Parameters[1].Type.Name == "CardPile" && x.Parameters[0].Type.Name != "CardModel");
// list overload - find Add with IEnumerable
foreach (var method in t.Methods.Where(x => x.Name == "Add"))
{
    var s = d.DecompileAsString(method.MetadataToken);
    if (s.Contains("skipVisuals") && s.Contains("IEnumerable") || method.Parameters.Count == 5 && method.Parameters[0].Type.Name.Contains("IEnumerable"))
    {
        Console.WriteLine($"=== {method.Parameters.Count} ===\n{s.Substring(0, Math.Min(3500, s.Length))}\n");
    }
}
