using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var decompiler = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
foreach (var full in new[] {
    "MegaCrit.Sts2.Core.Models.Relics.BagOfMarbles",
    "MegaCrit.Sts2.Core.Models.Relics.RedMask",
    "MegaCrit.Sts2.Core.Models.Relics.TwistedFunnel",
    "MegaCrit.Sts2.Core.Models.Relics.Pendulum",
    "MegaCrit.Sts2.Core.Models.Powers.PoisonPower"
})
{
    Console.WriteLine("\n========== " + full + " ==========");
    try {
        var name = new FullTypeName(full);
        Console.WriteLine(decompiler.DecompileTypeAsString(name));
    } catch (Exception ex) { Console.WriteLine("ERR " + ex.GetType().Name + ": " + ex.Message); }
}

Console.WriteLine("\n========== CombatManager turn hooks (filtered) ==========");
try {
    var text = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Combat.CombatManager"));
    var lines = text.Split('\n');
    bool dump = false; int ctx = 0;
    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i];
        if (line.Contains("StartTurn(") || line.Contains("SetupPlayerTurn(") || line.Contains("async Task StartTurn") || line.Contains("async Task SetupPlayerTurn"))
        { dump = true; ctx = 80; Console.WriteLine($"\n--- around line {i} ---"); }
        if (dump)
        {
            Console.WriteLine(line);
            ctx--;
            if (ctx <= 0) dump = false;
        }
        else if (line.Contains("AfterSideTurnStart") || line.Contains("AfterPlayerTurnStart") || line.Contains("BeforeSideTurnStart"))
            Console.WriteLine($"L{i}: {line}");
    }
} catch (Exception ex) { Console.WriteLine("ERR " + ex.Message); }
