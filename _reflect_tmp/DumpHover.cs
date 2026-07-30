using System;
using System.Linq;
using System.Reflection;

var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
foreach (var t in asm.GetTypes().Where(t => t.Name.Contains("HoverTip") || t.Name.Contains("Enchantment")))
{
  if (t.Name is "HoverTipFactory" or "Sharp" or "IHoverTip" || t.Name.Contains("Enchant"))
    Console.WriteLine("TYPE " + t.FullName);
}
var ht = asm.GetType("MegaCrit.Sts2.Core.HoverTips.HoverTipFactory");
Console.WriteLine("HoverTipFactory=" + ht);
if (ht != null)
{
  foreach (var m in ht.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance))
    Console.WriteLine(m);
}
var sharp = asm.GetTypes().FirstOrDefault(t => t.Name == "Sharp");
Console.WriteLine("Sharp=" + sharp);
if (sharp != null)
{
  foreach (var m in sharp.GetMembers(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly).Take(40))
    Console.WriteLine("  " + m);
}
// RelicModel ExtraHoverTips
var relic = asm.GetType("MegaCrit.Sts2.Core.Models.RelicModel");
foreach (var p in relic.GetProperties().Where(p => p.Name.Contains("Hover") || p.Name.Contains("Tip")))
  Console.WriteLine("RelicProp " + p);
