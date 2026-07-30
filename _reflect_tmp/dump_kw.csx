using Mono.Cecil;
var dll = args[0];
var asm = AssemblyDefinition.ReadAssembly(dll);
foreach (var t in asm.MainModule.Types.OrderBy(t => t.FullName))
{
    if (t.Name.Contains("Keyword", StringComparison.OrdinalIgnoreCase)
        || t.Name.Contains("Description", StringComparison.OrdinalIgnoreCase)
        || t.Name.Contains("AutoKeyword", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("TYPE " + t.FullName);
        foreach (var m in t.Methods)
            Console.WriteLine("  M " + m.Name);
        foreach (var f in t.Fields)
            Console.WriteLine("  F " + f.Name);
        foreach (var p in t.Properties)
            Console.WriteLine("  P " + p.Name);
    }
}
