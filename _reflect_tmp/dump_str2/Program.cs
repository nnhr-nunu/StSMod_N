using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    var inv = a.GetTypes().First(x => x.Name == "NRelicInventory");
    var add = inv.GetMethod("Add", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!;
    var il = add.GetMethodBody()!.GetILAsByteArray()!;
    for (int i=0;i<il.Length-4;i++) {
      if (il[i]==0x72 || il[i]==0x73) {
        try { Console.WriteLine(add.Module.ResolveString(BitConverter.ToInt32(il,i+1))); } catch {}
      }
    }
  }
}
