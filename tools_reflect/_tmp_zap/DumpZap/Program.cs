using Mono.Cecil;
using Mono.Cecil.Cil;
var asm = AssemblyDefinition.ReadAssembly(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var burn = asm.MainModule.GetType("MegaCrit.Sts2.Core.Models.Cards.Burn");
Console.WriteLine("Burn base=" + burn.BaseType);
foreach (var m in burn.Methods.Where(m => m.HasBody))
{
  Console.WriteLine("--- " + m.Name);
  foreach (var i in m.Body.Instructions)
    if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt || i.OpCode == OpCodes.Ldstr || i.OpCode.Name.StartsWith("ldc"))
      Console.WriteLine("  " + i);
}
// CardCmd.Transform signature
var cc = asm.MainModule.GetType("MegaCrit.Sts2.Core.Commands.CardCmd");
foreach (var m in cc.Methods.Where(m => m.Name.Contains("Transform") || m.Name.Contains("Add")))
  Console.WriteLine("CardCmd." + m);
// Hook AfterCardAdded / AfterCardObtained
var hook = asm.MainModule.GetType("MegaCrit.Sts2.Core.Hooks.Hook");
foreach (var m in hook.Methods.Where(m => m.Name.Contains("Card") && (m.Name.Contains("Add") || m.Name.Contains("Create") || m.Name.Contains("Obtain") || m.Name.Contains("Pile"))))
  Console.WriteLine("Hook." + m.Name);
