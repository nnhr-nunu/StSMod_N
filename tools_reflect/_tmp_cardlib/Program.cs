using Mono.Cecil;
using Mono.Cecil.Cil;
var sts2 = AssemblyDefinition.ReadAssembly(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = sts2.MainModule.GetType("MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen");
var m = t.Methods.First(x => x.Name == "RefreshOptions");
foreach (var i in m.Body.Instructions)
{
  if (i.Offset >= 0xD0 && i.Offset <= 0x1B0)
    Console.WriteLine(i.Offset.ToString("X4") + ": " + i);
}

// Find UNKNOWN.title in extracted loc
