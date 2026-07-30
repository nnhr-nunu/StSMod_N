using System.Reflection;
var asm = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpMonsterVisuals\bin\Release\net9.0\sts2.dll");
var player = asm.GetType("MegaCrit.Sts2.Core.Entities.Players.Player")!;
foreach (var p in player.GetProperties(BindingFlags.Public|BindingFlags.Instance)
  .Where(p => p.Name.Contains("Deck", StringComparison.OrdinalIgnoreCase)
           || p.Name.Contains("Card", StringComparison.OrdinalIgnoreCase)
           || p.Name.Contains("Pile", StringComparison.OrdinalIgnoreCase))
  .OrderBy(p => p.Name))
  Console.WriteLine(p.PropertyType.Name + " " + p.Name);

var deck = asm.GetTypes().Where(t => t.Name.Contains("Deck") && t.Namespace?.Contains("Entities") == true).Take(20);
foreach (var t in deck) Console.WriteLine("TYPE "+t.FullName);
