using Mono.Cecil;
using Mono.Cecil.Cil;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var m = ModuleDefinition.ReadModule(dll);

var ncard = m.Types.First(x => x.Name == "NCard");
var method = ncard.Methods.First(x => x.Name == "UpdatePortrait");
int n = 0;
foreach (var i in method.Body.Instructions)
{
    Console.WriteLine($"{n++:000} {i.OpCode.Name} {i.Operand}");
}
