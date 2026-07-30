using System;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
class P {
static void Main() {
var path = @"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.8\lib\net9.0\BaseLib.dll";
var d = new CSharpDecompiler(path, new DecompilerSettings{ThrowOnAssemblyResolveErrors=false});
Console.WriteLine(d.DecompileTypeAsString(new FullTypeName("BaseLib.Utils.CustomAnimation")));
}
}
