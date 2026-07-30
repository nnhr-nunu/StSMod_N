using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
var method = torch.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .First(x => x.Name == "AfterAddedToRoom");
var attr = method.GetCustomAttributesData().First(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
var body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
var il = body.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("TorchHead AfterAddedToRoom:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F or 0x73)
    {
        try
        {
            if (il[i] == 0x73)
            {
                var type = body.Module.ResolveType(BitConverter.ToInt32(il, i + 1));
                Console.WriteLine($"  TYPE {type?.FullName}");
            }
            else
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
            }
        }
        catch { }
        i += 4;
    }
}

// HittableEnemies lambda
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;
foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (!m.Name.Contains("67_0")) continue;
        il = m.GetMethodBody()!.GetILAsByteArray()!;
        Console.WriteLine($"\n{m.Name}:");
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

// CombatState - when combat ends check enemies
foreach (var m in cs.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (!m.Name.Contains("AllEnemies") && !m.Name.Contains("Hittable") && !m.Name.Contains("IsCombat"))
        continue;
    Console.WriteLine($"CombatState.{m.Name}");
}
