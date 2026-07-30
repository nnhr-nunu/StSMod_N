using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    foreach (var typeName in new[]{"NInspectCardScreen","NHoverTipSet","HoverTipAlignment"}) {
      var t = a.GetTypes().First(x => x.Name == typeName);
      Console.WriteLine("=== " + typeName + " ===");
      foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(x=>x.Name))
        Console.WriteLine(m.IsStatic?"static ":"" + m.Name + " : " + m.ReturnType.Name);
      foreach (var f in t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly).OrderBy(x=>x.Name))
        Console.WriteLine("field " + f.FieldType.Name + " " + f.Name);
      foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly).OrderBy(x=>x.Name))
        Console.WriteLine("prop " + p.PropertyType.Name + " " + p.Name);
      if (t.IsEnum) foreach (var n in Enum.GetNames(t)) Console.WriteLine("enum " + n + "=" + (int)Enum.Parse(t,n));
    }
  }
}
