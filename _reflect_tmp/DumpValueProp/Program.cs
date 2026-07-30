using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = a.GetType("MegaCrit.Sts2.Core.ValueProps.ValueProp");
Console.WriteLine(string.Join("\n", Enum.GetNames(t!).Select(n => $"{n}={(int)Enum.Parse(t,n)}")));
