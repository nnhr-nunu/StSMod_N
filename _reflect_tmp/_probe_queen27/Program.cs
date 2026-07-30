using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cs = a.GetType("MegaCrit.Sts2.Core.Combat.CombatState")!;

foreach (var nested in cs.GetNestedTypes(BindingFlags.NonPublic))
{
    foreach (var m in nested.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    {
        var mil = m.GetMethodBody()?.GetILAsByteArray();
        if (mil == null) continue;
        bool interesting = false;
        var lines = new System.Collections.Generic.List<string>();
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    lines.Add($"{member!.DeclaringType?.Name}.{member.Name}");
                    if (member.Name.Contains("Alive") || member.Name.Contains("Dead") || member.Name.Contains("Hittable") || member.Name.Contains("Minion") || member.Name.Contains("Secondary"))
                        interesting = true;
                }
                catch { }
                i += 4;
            }
        }
        if (interesting)
        {
            Console.WriteLine($"{nested.Name}.{m.Name}:");
            foreach (var l in lines) Console.WriteLine($"  {l}");
        }
    }
}

// PowerModel get_OwnerIsSecondaryEnemy
var pm = a.GetType("MegaCrit.Sts2.Core.Models.PowerModel")!;
var ose = pm.GetMethod("get_OwnerIsSecondaryEnemy", BindingFlags.Public | BindingFlags.Instance)!;
var il = ose.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nPowerModel.OwnerIsSecondaryEnemy:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = ose.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
