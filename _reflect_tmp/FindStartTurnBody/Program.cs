using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var outDir = @"D:\Dev\antigravity\StSMod_N\_reflect_tmp\sts2_decomp";
Directory.CreateDirectory(outDir);
var decompiler = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var text = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Combat.CombatManager"));
File.WriteAllText(Path.Combine(outDir, "CombatManager.cs"), text);

// Find all Hook.*Turn* call sites with surrounding context
var lines = text.Split('\n');
for (int i = 0; i < lines.Length; i++)
{
    if (Regex.IsMatch(lines[i], @"Hook\.(Before|After).*(Turn|Side)"))
    {
        Console.WriteLine($"\n==== L{i+1} ====");
        for (int j = Math.Max(0, i-15); j <= Math.Min(lines.Length-1, i+5); j++)
            Console.WriteLine($"{j+1}: {lines[j].TrimEnd()}");
    }
}

// Also dump method signatures containing StartTurn
Console.WriteLine("\n==== methods with StartTurn in name ====");
foreach (Match m in Regex.Matches(text, @"(?:public|private|internal|protected).{0,80}StartTurn[^\n]*"))
    Console.WriteLine(m.Value);
