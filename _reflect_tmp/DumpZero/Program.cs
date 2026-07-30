using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

class P {
  static void Main() {
    using var fs = File.OpenRead(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    using var pe = new PEReader(fs);
    var md = pe.GetMetadataReader();
    foreach (var th in md.TypeDefinitions) {
      var t = md.GetTypeDefinition(th);
      var name = md.GetString(t.Name);
      if (name != "TemporaryStrengthPower" && name != "DarkShackles" && name != "StrengthPower") continue;
      Console.WriteLine("=== " + md.GetString(t.Namespace) + "." + name + " ===");
      foreach (var mh in t.GetMethods()) {
        var m = md.GetMethodDefinition(mh);
        Console.WriteLine("  M " + md.GetString(m.Name));
      }
      foreach (var ph in t.GetProperties()) {
        var p = md.GetPropertyDefinition(ph);
        Console.WriteLine("  P " + md.GetString(p.Name));
      }
    }
  }
}
