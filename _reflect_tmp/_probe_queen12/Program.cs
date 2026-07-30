using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;

foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        if (!m.Name.Contains("b__") && !m.Name.Contains("Hittable")) continue;
        var il = m.GetMethodBody()?.GetILAsByteArray();
        if (il == null || il.Length > 100) continue;
        Console.WriteLine($"\n{nested.Name}.{m.Name}:");
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

// TorchHead AfterAddedToRoom - minion power?
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
foreach (var m in torch.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == "AfterAddedToRoom"))
{
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = m;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = body.GetMethodBody()?.GetILAsByteArray();
    Console.WriteLine("\nTorchHead AfterAddedToRoom:");
    for (var i = 0; i < il!.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                if (!n.Contains("Async") && !n.Contains("Await") && !n.Contains("Enumerator"))
                    Console.WriteLine($"  {n}");
            }
            catch { }
            i += 4;
        }
    }
}
