using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
        var a = Assembly.LoadFrom(path);

        void DumpType(string name)
        {
            var t = a.GetTypes().FirstOrDefault(x => x.Name == name || x.FullName == name);
            if (t == null) { Console.WriteLine("MISSING " + name); return; }
            Console.WriteLine("=== " + t.FullName + " ===");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (m.Name.Contains("X") || m.Name.Contains("Cost") || m.Name.Contains("Play") || m.Name == "OnPlay")
                    Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" +
                                      string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
            }
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                Console.WriteLine("  prop " + p.PropertyType.Name + " " + p.Name);
        }

        DumpType("Whirlwind");
        DumpType("Skewer");
        DumpType("Tempest");
        DumpType("CardEnergyCost");
        DumpType("CardModel");

        // CanonicalEnergyCost for whirlwind - look at fields
        var ww = a.GetType("MegaCrit.Sts2.Core.Models.Cards.Whirlwind");
        if (ww != null)
        {
            foreach (var f in ww.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                Console.WriteLine("WW field " + f.FieldType.Name + " " + f.Name);
        }
    }
}
