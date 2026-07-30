using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var decompiler = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });

foreach (var full in new[] {
    "MegaCrit.Sts2.Core.Models.Powers.VulnerablePower",
    "MegaCrit.Sts2.Core.Models.Powers.WeakPower",
    "MegaCrit.Sts2.Core.Commands.PowerCmd"
})
{
    Console.WriteLine("\n========== " + full + " ==========");
    var text = decompiler.DecompileTypeAsString(new FullTypeName(full));
    if (full.EndsWith("PowerCmd"))
    {
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("SkipNext") || lines[i].Contains("AmountOnTurnStart") ||
                lines[i].Contains("ApplyInternal") || lines[i].Contains("static async Task") && lines[i].Contains("Apply") ||
                lines[i].Contains("Duration") || lines[i].Contains("Decrement"))
            {
                Console.WriteLine($"L{i}: {lines[i]}");
            }
        }
        // dump Apply method
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("public static async Task") && (lines[i].Contains("Apply(") || lines[i].Contains("Apply<")))
            {
                Console.WriteLine($"\n--- method L{i} ---");
                for (int j = i; j < Math.Min(lines.Length, i+80); j++) Console.WriteLine(lines[j]);
            }
        }
    }
    else
    {
        // show AfterSideTurnStart and related
        Console.WriteLine(text.Length > 3500 ? text.Substring(0, 3500) : text);
    }
}

// PowerModel SkipNextDurationTick usage
Console.WriteLine("\n========== PowerModel SkipNextDurationTick context ==========");
var pm = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Models.PowerModel"));
var plines = pm.Split('\n');
for (int i = 0; i < plines.Length; i++)
{
    if (plines[i].Contains("SkipNextDurationTick") || plines[i].Contains("AmountOnTurnStart") || plines[i].Contains("Duration"))
    {
        for (int j = Math.Max(0,i-2); j < Math.Min(plines.Length, i+8); j++) Console.WriteLine($"L{j}: {plines[j]}");
        Console.WriteLine("---");
    }
}
