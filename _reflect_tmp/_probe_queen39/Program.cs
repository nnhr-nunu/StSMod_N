using System;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
foreach (var p in ap.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    var get = p.GetGetMethod(nonPublic: true);
    Console.WriteLine($"{p.Name} getter={get?.Name} public={get?.IsPublic} private={get?.IsPrivate} family={get?.IsFamily}");
}
