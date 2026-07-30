using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;

foreach (var m in hook.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "ModifyUnblockedDamageTarget"))
{
    Console.WriteLine($"sig: {m}");
    var il = m.GetMethodBody()?.GetILAsByteArray()!;
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
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
