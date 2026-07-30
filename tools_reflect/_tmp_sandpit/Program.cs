using System;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var decompiler = new CSharpDecompiler(path, new DecompilerSettings());
foreach (var name in new[] {
  "MegaCrit.Sts2.Core.Models.Powers.SandpitPower",
})
{
  try {
    Console.WriteLine(decompiler.DecompileTypeAsString(new FullTypeName(name)));
  } catch (Exception ex) { Console.WriteLine(ex); }
}
