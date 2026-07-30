using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
        MethodInfo? body = m;
        if (attr != null)
            body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var mil = body?.GetMethodBody()?.GetILAsByteArray();
        if (mil == null) continue;
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = body!.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    if (member?.DeclaringType?.Name == "Queen" || member?.Name.Contains("HasAmalgam") == true)
                        Console.WriteLine($"{t.Name}.{m.Name} -> Queen.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

// MinionPower - does minion count for combat end?
var mp = a.GetType("MegaCrit.Sts2.Core.Models.Powers.MinionPower")!;
foreach (var m in mp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    if (m.Name.Contains("Stop") || m.Name.Contains("Remove") || m.Name.Contains("Death"))
        Console.WriteLine($"MinionPower.{m.Name}");
}

var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
var ir = ap.GetMethod("get_IsReviving", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
var il = ir.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nAdaptablePower.IsReviving:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = ir.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
