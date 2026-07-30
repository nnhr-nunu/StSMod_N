using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = a.GetType("MegaCrit.Sts2.Core.Models.PowerModel")!;
foreach (var p in t.GetProperties().Where(p => p.Name.Contains("Icon") || p.Name.Contains("Path")))
    Console.WriteLine($"{p.Name} : {p.PropertyType.Name}");

var slow = a.GetType("MegaCrit.Sts2.Core.Models.Powers.SlowPower")!;
foreach (var m in slow.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"slow method: {m.Name}");
