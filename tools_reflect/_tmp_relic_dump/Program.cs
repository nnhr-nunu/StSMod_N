using Mono.Cecil;

static class Program
{
    static void Main()
    {
        var baselib = @"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.6\lib\net9.0\BaseLib.dll";
        var bl = ModuleDefinition.ReadModule(baselib);
        var patch = bl.Types.First(t => t.Name == "StarterUpgradePatches");
        void DumpAttrs(ICustomAttributeProvider p, string label)
        {
            foreach (var ca in p.CustomAttributes)
            {
                Console.WriteLine($"{label} attr {ca.AttributeType.FullName}");
                for (int i = 0; i < ca.ConstructorArguments.Count; i++)
                    Console.WriteLine($"  ctor[{i}] {ca.ConstructorArguments[i].Type.Name} = {ca.ConstructorArguments[i].Value}");
                foreach (var prop in ca.Properties)
                    Console.WriteLine($"  prop {prop.Name} = {prop.Argument.Value}");
                foreach (var f in ca.Fields)
                    Console.WriteLine($"  field {f.Name} = {f.Argument.Value}");
            }
        }
        DumpAttrs(patch, "class");
        foreach (var m in patch.Methods)
            DumpAttrs(m, m.Name);

        // PotionUsage + TargetType from BloodPotion
        var dll = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
        var sts = ModuleDefinition.ReadModule(dll);
        var bp = sts.Types.SelectMany(t => new[]{t}.Concat(t.NestedTypes)).First(t => t.Name == "BloodPotion");
        foreach (var name in new[] { "get_Usage", "get_TargetType" })
        {
            var m = bp.Methods.First(x => x.Name == name);
            Console.WriteLine(name + ":");
            foreach (var i in m.Body.Instructions)
                Console.WriteLine($"  {i.OpCode.Name} {i.Operand}");
        }

        // CustomPotion AutoAdd default
        var cpm = bl.Types.First(t => t.Name == "CustomPotionModel");
        var auto = cpm.Properties.FirstOrDefault(p => p.Name == "AutoAdd")?.GetMethod
                   ?? cpm.Methods.FirstOrDefault(m => m.Name == "get_AutoAdd");
        Console.WriteLine("\nCustomPotionModel.get_AutoAdd:");
        if (auto?.HasBody == true)
            foreach (var i in auto.Body.Instructions)
                Console.WriteLine($"  {i.OpCode.Name} {i.Operand}");
    }
}
