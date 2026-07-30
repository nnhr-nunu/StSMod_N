using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
foreach (var t in a.GetTypes().Where(t => (t.Name.Contains("Cost") || t.Name.Contains("Unplayable") || t.Name.Contains("Slash")) && t.FullName!.Contains("Cards")).OrderBy(t=>t.FullName).Take(100))
    Console.WriteLine(t.FullName);
