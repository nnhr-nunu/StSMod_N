using System;
using System.Linq;
using System.Reflection;
class Program {
  static void Main() {
    var baseLib = @"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.6\lib\net9.0\BaseLib.dll";
    var sts2 = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
    var a = Assembly.LoadFrom(baseLib);
    foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Description") || t.Name.Contains("Float") || t.Name.Contains("CombatText"))) {
      Console.WriteLine("BL " + t.FullName);
      foreach (var m in t.GetMembers(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Take(50))
        Console.WriteLine("  " + m.MemberType + " " + m);
    }
    var sa = Assembly.LoadFrom(sts2);
    foreach (var name in new[]{"MegaCrit.Sts2.Core.Models.CardModel","MegaCrit.Sts2.Core.Nodes.Combat.NCreature","MegaCrit.Sts2.Core.Commands.CreatureCmd","MegaCrit.Sts2.Core.Commands.CombatCmd","MegaCrit.Sts2.Core.Commands.VfxCmd"}) {
      var t = sa.GetType(name);
      Console.WriteLine("=== " + name + " " + (t!=null) + " ===");
      if (t==null) continue;
      foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Where(p => p.Name.Contains("Target") || p.Name.Contains("Drag") || p.Name.Contains("Hover") || p.Name.Contains("Aim") || p.Name.Contains("Float") || p.Name.Contains("Text") || p.Name.Contains("Talk")))
        Console.WriteLine("  P " + p.PropertyType.Name + " " + p.Name);
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m => m.Name.Contains("Float") || m.Name.Contains("Text") || m.Name.Contains("Talk") || m.Name.Contains("Speech") || m.Name.Contains("Banner") || m.Name.Contains("Popup") || m.Name.Contains("Show")))
        Console.WriteLine("  M " + m);
    }
    Console.WriteLine("=== search float-ish types ===");
    foreach (var t in sa.GetTypes().Where(t => t.Name.Contains("Float", StringComparison.OrdinalIgnoreCase) || t.Name.Contains("CombatText") || t.Name.Contains("DamageNumber") || t.Name.Contains("ThoughtBubble") || t.Name.Contains("SpeechBubble") || t.Name.Contains("Overhead"))) {
      Console.WriteLine("ST " + t.FullName);
    }
  }
}
