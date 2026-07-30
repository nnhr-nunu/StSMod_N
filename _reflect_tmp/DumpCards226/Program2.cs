using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
var dll = @"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.8\lib\net9.0\BaseLib.dll";
var d = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
foreach (var name in new[]{"CustomTemporaryPowerModelWrapper`2","CustomTemporaryPowerModel"}) {
  try {
    var s = d.DecompileTypeAsString(new FullTypeName("BaseLib.Abstracts."+name));
    Console.WriteLine("=== "+name+" ===");
    Console.WriteLine(s.Length > 4000 ? s[..4000] : s);
  } catch (Exception e) { Console.WriteLine(name+" err "+e.Message); }
}
