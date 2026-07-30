using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var am = a.GetType("MegaCrit.Sts2.Core.Models.AbstractModel")!;

foreach (var m in am.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == "ModifyUnblockedDamageTarget"))
{
    var il = m.GetMethodBody()?.GetILAsByteArray()!;
    Console.WriteLine("AbstractModel.ModifyUnblockedDamageTarget default:");
    for (var i = 0; i < il.Length; i++)
        Console.WriteLine($"  {il[i]:X2}");
}
