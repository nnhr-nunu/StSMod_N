using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var t in a.GetTypes().Where(x => x.FullName?.Contains("Queen") == true || x.FullName?.Contains("TorchHead") == true))
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
    {
        try
        {
            var il = m.GetMethodBody()?.GetILAsByteArray();
            if (il == null) continue;
            for (var i = 0; i < il.Length; i++)
            {
                if (il[i] is 0x28 or 0x6F or 0x73)
                {
                    try
                    {
                        var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                        if (member?.DeclaringType?.Name == "AdaptablePower")
                            Console.WriteLine($"{t.Name}.{m.Name} -> AdaptablePower");
                    }
                    catch { }
                    i += 4;
                }
            }
        }
        catch { }
    }
}

// Who has ShouldStopCombatFromEnding override?
foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        if (m.Name != "ShouldStopCombatFromEnding") continue;
        if (m.DeclaringType?.Name == "AbstractModel") continue;
        Console.WriteLine($"ShouldStopCombatFromEnding override: {t.FullName}");
    }
}
