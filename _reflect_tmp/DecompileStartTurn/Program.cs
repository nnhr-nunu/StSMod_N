using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var decompiler = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var text = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Combat.CombatManager"));
// Extract StartTurn and SetupPlayerTurn method bodies more carefully
var markers = new[] { "public async Task StartTurn", "private async Task StartTurn", "async Task StartTurn", "public async Task SetupPlayerTurn", "private async Task SetupPlayerTurn", "async Task SetupPlayerTurn" };
var lines = text.Split('\n');
for (int i = 0; i < lines.Length; i++)
{
    if (lines[i].Contains("Task StartTurn") || lines[i].Contains("Task SetupPlayerTurn") ||
        (lines[i].Contains("StartTurn(") && lines[i].Contains("Task")))
    {
        Console.WriteLine($"\n===== method near L{i}: {lines[i].Trim()} =====");
        for (int j = i; j < Math.Min(lines.Length, i + 120); j++)
            Console.WriteLine(lines[j]);
    }
}

Console.WriteLine("\n===== Hook.AfterPlayerTurnStart / AfterSideTurnStart =====");
var hook = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Hooks.Hook"));
var hlines = hook.Split('\n');
for (int i = 0; i < hlines.Length; i++)
{
    if (hlines[i].Contains("AfterPlayerTurnStart(") || hlines[i].Contains("AfterSideTurnStart(") || hlines[i].Contains("BeforeSideTurnStart("))
    {
        Console.WriteLine($"\n--- L{i} ---");
        for (int j = i; j < Math.Min(hlines.Length, i + 40); j++)
        {
            Console.WriteLine(hlines[j]);
            if (j > i && hlines[j].Trim() == "}" && hlines[j-1].Contains("return")) break;
            if (j > i + 5 && hlines[j].Contains("public static") ) break;
        }
    }
}

// Also check PowerCmd.Apply for AmountOnTurnStart / skip decrement behavior
Console.WriteLine("\n===== PowerModel AmountOnTurnStart / just applied =====");
var pm = decompiler.DecompileTypeAsString(new FullTypeName("MegaCrit.Sts2.Core.Models.PowerModel"));
foreach (var line in pm.Split('\n'))
{
    if (line.Contains("AmountOnTurnStart") || line.Contains("JustApplied") || line.Contains("Skip") || line.Contains("TurnStart"))
        Console.WriteLine(line);
}
