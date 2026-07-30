using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes().Where(x => x.Name.Contains("Adaptable") || x.Name.Contains("Experiment")))
    Console.WriteLine(t.FullName);

var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
foreach (var f in ap.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    Console.WriteLine($"AdaptablePower field {f.Name} {f.FieldType.Name}");
foreach (var p in ap.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"AdaptablePower prop {p.Name}");

// MonsterModel ShouldDisappearFromDoom default
var mm = a.GetType("MegaCrit.Sts2.Core.Models.MonsterModel")!;
var sd = mm.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.Instance)!;
var il = sd.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("MonsterModel.ShouldDisappearFromDoom: " + BitConverter.ToString(il));

// Queen override?
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var qsd = queen.GetMethod("get_ShouldDisappearFromDoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
Console.WriteLine("Queen has own ShouldDisappearFromDoom: " + (qsd != null));

// CombatState HittableEnemies filter full IL
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
var getH = cs.GetMethod("get_HittableEnemies", BindingFlags.Public | BindingFlags.Instance)!;
il = getH.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nget_HittableEnemies:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = getH.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
