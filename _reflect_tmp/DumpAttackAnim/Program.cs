using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.IO;

class Program
{
    static void Main()
    {
        var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
        var a = Assembly.LoadFrom(path);

        void Dump(string fullOrName)
        {
            var t = a.GetTypes().FirstOrDefault(x => x.FullName == fullOrName || x.Name == fullOrName);
            if (t == null) { Console.WriteLine("MISSING " + fullOrName); return; }
            Console.WriteLine("\n=== " + t.FullName + " ===");
            foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                Console.WriteLine("  field " + f.FieldType.Name + " " + f.Name);
            foreach (var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                Console.WriteLine("  prop " + p.PropertyType.Name + " " + p.Name);
            foreach (var m in t.GetMethods(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
            {
                var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
                Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
            }
        }

        Dump("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand");
        Dump("NCreature");

        // methods containing Anim / Wait / Replay on CardModel
        var cm = a.GetType("MegaCrit.Sts2.Core.Models.CardModel");
        if (cm != null)
        {
            Console.WriteLine("\n=== CardModel Anim/Replay ===");
            foreach (var m in cm.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)
                .Where(m => m.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase)
                         || m.Name.Contains("Anim", StringComparison.OrdinalIgnoreCase)
                         || m.Name.Contains("EnergyX", StringComparison.OrdinalIgnoreCase)
                         || m.Name.Contains("ResolveEnergy", StringComparison.OrdinalIgnoreCase)))
            {
                var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
                Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
            }
            foreach (var p in cm.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                .Where(p => p.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase)
                         || p.Name.Contains("Anim", StringComparison.OrdinalIgnoreCase)))
                Console.WriteLine("  prop " + p.PropertyType.Name + " " + p.Name);
        }

        // Find types with OnlyPlayAnimOnce
        foreach (var t in a.GetTypes())
        {
            foreach (var m in t.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.DeclaredOnly))
            {
                if (m.Name.Contains("OnlyPlayAnim", StringComparison.OrdinalIgnoreCase)
                    || m.Name.Contains("WaitForAnim", StringComparison.OrdinalIgnoreCase)
                    || m.Name.Contains("PlayAnimOnce", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine($"FOUND {t.FullName}.{m.Name}");
            }
            foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.DeclaredOnly))
            {
                if (f.Name.Contains("OnlyPlayAnim", StringComparison.OrdinalIgnoreCase)
                    || f.Name.Contains("playAnim", StringComparison.OrdinalIgnoreCase)
                    || f.Name.Contains("AnimOnce", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine($"FIELD {t.FullName}.{f.Name} : {f.FieldType.Name}");
            }
        }
    }
}
