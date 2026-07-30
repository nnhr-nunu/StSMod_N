using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var zap = asm.GetType("MegaCrit.Sts2.Core.Models.Monsters.Zapbot");
    foreach (var m in zap.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)) {
      Console.WriteLine("M " + m);
      var body = m.GetMethodBody();
      if (body == null) continue;
      var il = body.GetILAsByteArray();
      Console.WriteLine("  IL " + BitConverter.ToString(il));
    }
    foreach (var p in zap.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
      Console.WriteLine("P " + p + " CanRead=" + p.CanRead);
    
    // AttackCommand TargetingRandomOpponents signature details
    var ac = asm.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand");
    foreach (var m in ac.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly))
      if (m.Name.Contains("Target") || m.Name.Contains("With") || m.Name.Contains("Execute") || m.Name=="FromMonster")
        Console.WriteLine("AC " + m);
    
    // Player pets list
    var player = asm.GetType("MegaCrit.Sts2.Core.Entities.Players.Player");
    foreach (var p in player.GetProperties()) if (p.Name.Contains("Pet")) Console.WriteLine("Player."+p);
    
    // HighVoltagePower
    var hv = asm.GetTypes().FirstOrDefault(t => t.Name == "HighVoltagePower");
    Console.WriteLine("HV " + hv);
  }
}
