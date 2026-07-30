using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;

Console.WriteLine("TorchHeadAmalgam members:");
foreach (var m in torch.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    Console.WriteLine($"  {m.MemberType} {m.Name}");

foreach (var m in torch.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
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
                    if (member?.Name.Contains("Power") == true && member.Name.Contains("Apply"))
                        Console.WriteLine($"{m.Name}: {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
    catch { }
}
