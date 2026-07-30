using System;
using System.Linq;
using Mono.Cecil;

var path = args[0];
var asm = AssemblyDefinition.ReadAssembly(path);
foreach (var t in asm.MainModule.Types.Where(t => t.Name.Contains(\"CustomCard\") || t.Name.Contains(\"CardPool\") || t.Name.Contains(\"Portrait\")).Take(40))
{
    Console.WriteLine(t.FullName);
    foreach (var m in t.Methods.Take(30))
        Console.WriteLine(\"  M \" + m.Name);
    foreach (var p in t.Properties)
        Console.WriteLine(\"  P \" + p.Name);
}
