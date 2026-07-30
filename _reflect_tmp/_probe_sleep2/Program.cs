using System;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var pt = a.GetType("MegaCrit.Sts2.Core.Entities.Powers.PowerType")!;
foreach (var name in Enum.GetNames(pt))
    Console.WriteLine($"{name} = {Enum.Parse(pt, name)}");
