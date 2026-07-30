using Mono.Cecil;
var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".nuget", "packages", "alchyr.sts2.baselib", "3.3.6", "lib", "net9.0", "BaseLib.dll");
var asm = AssemblyDefinition.ReadAssembly(path);
foreach (var t in asm.MainModule.Types.Where(t => t.Name.Contains("CustomCalculated") || t.Name == "ExtraDamageVar"))
{
    Console.WriteLine(t.FullName + " : " + t.BaseType);
    foreach (var m in t.Methods.Where(m => m.Name.Contains("WithMultiplier") || m.Name == ".ctor"))
        Console.WriteLine("  " + m.FullName);
}
