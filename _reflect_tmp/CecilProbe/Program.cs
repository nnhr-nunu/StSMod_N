using Mono.Cecil;
var path = args[0];
var asm = AssemblyDefinition.ReadAssembly(path);
foreach (var t in asm.MainModule.Types)
{
    if (!(t.Name.Contains("CustomCard") || t.Name.Contains("CardPool") || t.Name == "PoolAttribute")) continue;
    Console.WriteLine("=== " + t.FullName);
    foreach (var p in t.Properties) Console.WriteLine("  P " + p.Name + " : " + p.PropertyType);
    foreach (var m in t.Methods.Where(m => !m.IsGetter && !m.IsSetter))
        if (m.Name.Contains("Portrait") || m.Name.Contains("Pool") || m.Name.Contains("Reward") || m.Name.Contains("Generate") || m.Name.Contains("Add") || m.Name == ".ctor")
            Console.WriteLine("  M " + m.FullName);
}
