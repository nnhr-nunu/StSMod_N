using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.8\lib\net9.0\BaseLib.dll";
var d = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var s = d.DecompileTypeAsString(new FullTypeName("BaseLib.Abstracts.CustomTemporaryPowerModel"));
foreach (var line in s.Split('\n'))
{
    if (line.Contains("UntilEndOfOtherSideTurn") || line.Contains("LastForXExtraTurns") ||
        line.Contains("StackType") || line.Contains("class CustomTemporaryPowerModelWrapper"))
        Console.WriteLine(line.TrimEnd());
}
