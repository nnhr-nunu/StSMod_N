using Mono.Cecil;
using Mono.Cecil.Cil;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = AssemblyDefinition.ReadAssembly(path);
var rewardsSet = asm.MainModule.Types.First(t => t.FullName == "MegaCrit.Sts2.Core.Rewards.RewardsSet");
var sm = rewardsSet.NestedTypes.First(t => t.Name == "<Offer>d__33");
var moveNext = sm.Methods.First(m => m.Name == "MoveNext");
foreach (var i in moveNext.Body.Instructions)
{
    var op = i.Operand;
    var s = op switch
    {
        string str => $"\"{str}\"",
        MethodReference mr => mr.FullName,
        FieldReference fr => fr.FullName,
        Instruction target => $"->IL_{target.Offset:X4}",
        _ => ""
    };
    Console.WriteLine($"IL_{i.Offset:X4}: {i.OpCode} {s}");
}
