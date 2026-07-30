using System;
using System.Linq;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var a = Assembly.LoadFrom(dll);
var t = a.GetType("MegaCrit.Sts2.Core.ValueProps.ValueProp");
foreach (var n in Enum.GetNames(t!)) Console.WriteLine($"{n}={(ulong)Convert.ToInt64(Enum.Parse(t,n))}");
var d = new CSharpDecompiler(dll, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
// Find cards that lose HP directly
foreach (var name in new[]{"MegaCrit.Sts2.Core.Models.Cards.Hemokinesis","MegaCrit.Sts2.Core.Commands.CreatureCmd"}) {
  try {
    var text = d.DecompileTypeAsString(new FullTypeName(name));
    if (name.Contains("CreatureCmd")) {
      foreach (var line in text.Split('\n'))
        if (line.Contains("LoseHp") || line.Contains("Unblockable") || line.Contains("public static async Task Damage") || line.Contains("public static async Task Lose"))
          Console.WriteLine("CC:"+line.Trim());
    } else Console.WriteLine(text.Substring(0, Math.Min(2500, text.Length)));
  } catch (Exception ex) { Console.WriteLine(name+": "+ex.Message); }
}
