using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var mp = a.GetType("MegaCrit.Sts2.Core.Models.Powers.MinionPower")!;

foreach (var method in mp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    var mil = method.GetMethodBody()?.GetILAsByteArray();
    if (mil == null || mil.Length > 20) continue;
    Console.WriteLine($"{method.Name}: {BitConverter.ToString(mil)}");
}

// Queen AfterAddedToRoom - sets amalgam
var queen = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.Queen")!;
var aa = queen.GetMethod("AfterAddedToRoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
var attr = aa.GetCustomAttributesData().First(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
var body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
var il = body.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nQueen AfterAddedToRoom (power apply):");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F or 0x73)
    {
        try
        {
            if (il[i] == 0x73)
            {
                var t = body.Module.ResolveType(BitConverter.ToInt32(il, i + 1));
                Console.WriteLine($"  TYPE {t?.FullName}");
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
