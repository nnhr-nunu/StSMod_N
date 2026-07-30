using Mono.Cecil;
using Mono.Cecil.Cil;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = AssemblyDefinition.ReadAssembly(path);
var t = asm.MainModule.Types.First(x => x.FullName == "MegaCrit.Sts2.Core.Models.CardModel");
foreach (var m in t.Methods.Where(m => m.Name.Contains("Retain") || m.Name.Contains("Sly")))
{
    Console.WriteLine("==== " + m.FullName);
    if (!m.HasBody) continue;
    foreach (var i in m.Body.Instructions)
    {
        string e = i.OpCode.ToString();
        if (i.Operand is string s) e += " \"" + s + "\"";
        else if (i.Operand is MethodReference mr) e += " " + mr.DeclaringType.Name + "::" + mr.Name;
        else if (i.Operand is FieldReference fr) e += " " + fr.Name;
        else if (i.Operand != null) e += " " + i.Operand;
        Console.WriteLine("  " + e);
    }
}

// decode beforeDescription enum values via CardKeyword names if possible
Console.WriteLine("\n==== CardKeyword enum ====");
var kw = asm.MainModule.Types.First(x => x.FullName == "MegaCrit.Sts2.Core.Entities.Cards.CardKeyword");
foreach (var f in kw.Fields.Where(f => f.IsStatic && f.IsLiteral))
    Console.WriteLine(f.Name + " = " + f.Constant);

// BaseLib AutoKeywordText patch target
Console.WriteLine("\n==== BaseLib AutoKeywordText harmony target ====");
var bl = AssemblyDefinition.ReadAssembly(@"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.6\lib\net9.0\BaseLib.dll");
var akt = bl.MainModule.Types.First(x => x.Name == "AutoKeywordText");
foreach (var ca in akt.CustomAttributes) Console.WriteLine("attr " + ca.AttributeType.Name + ": " + string.Join(", ", ca.ConstructorArguments.Select(a => a.Value)));
foreach (var m in akt.Methods)
foreach (var ca in m.CustomAttributes)
    Console.WriteLine(m.Name + " attr " + ca.AttributeType.Name + ": " + string.Join(" | ", ca.ConstructorArguments.Select(a => a.Value)));
