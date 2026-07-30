using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var powerCmd = a.GetType("MegaCrit.Sts2.Core.Commands.PowerCmd")!;
foreach (var m in powerCmd.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(x => x.Name == "Apply"))
{
    var ps = m.GetParameters();
    Console.WriteLine($"{m.Name} generic={m.IsGenericMethodDefinition} count={ps.Length} types={string.Join(",", ps.Select(p => p.ParameterType.Name))}");
}
