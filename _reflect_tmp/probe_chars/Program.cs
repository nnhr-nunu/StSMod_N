using System;
using System.Linq;
using System.Reflection;
var ass = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
foreach (var name in new[]{"Ironclad","Silent","Regent","Necrobinder","Defect","CharacterModel","PlaceholderCharacterModel"})
{
    var types = ass.GetTypes().Where(t => t.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).Take(15).ToList();
    Console.WriteLine("=== " + name + " ===");
    foreach (var t in types) Console.WriteLine(t.FullName);
}
Console.WriteLine("\n=== DemonForm etc ===");
foreach (var name in new[]{"DemonForm","SerpentForm","VoidForm","ReaperForm","EchoForm"})
{
    var t = ass.GetTypes().FirstOrDefault(x => x.Name == name);
    Console.WriteLine(name + " => " + (t?.FullName ?? "MISSING"));
}
