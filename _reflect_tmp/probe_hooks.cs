using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.IO;

var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
if (!File.Exists(dll)) {
  // try alternate
  var alts = Directory.GetFiles(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2", "sts2.dll", SearchOption.AllDirectories);
  Console.WriteLine("alts: " + string.Join(", ", alts));
  dll = alts.FirstOrDefault() ?? dll;
}
Console.WriteLine("dll=" + dll + " exists=" + File.Exists(dll));

var a = Assembly.LoadFrom(dll);
foreach (var tname in new[]{"BagOfPreparation","Hook","PoisonPower","ArtifactPower","SlowPower"})
{
  var t = a.GetTypes().FirstOrDefault(x => x.Name == tname);
  if (t == null) { Console.WriteLine(tname + ": NOT FOUND"); continue; }
  Console.WriteLine("\n=== " + t.FullName + " ===");
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
  {
    if (m.Name.Contains("Turn") || m.Name.Contains("Combat") || m.Name.Contains("Draw") || m.Name.Contains("After") || m.Name.Contains("Before") || m.Name.Contains("Side"))
      Console.WriteLine("  " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
  }
}

// Find Hook methods related to turn start order - look for source if available via method names
var hook = a.GetTypes().FirstOrDefault(x => x.Name == "Hook");
if (hook != null)
{
  Console.WriteLine("\n=== Hook turn-related ===");
  foreach (var m in hook.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly).OrderBy(m => m.Name))
  {
    if (m.Name.Contains("Turn") || m.Name.Contains("CombatStart") || m.Name.Contains("Side"))
      Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
  }
}
