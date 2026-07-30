using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

int count = 0;
foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var mil = m.GetMethodBody()?.GetILAsByteArray();
        if (mil == null || mil.Length > 40) continue;
        bool sec = false;
        for (var i = 0; i < mil.Length; i++)
        {
            if (mil[i] is 0x28 or 0x6F)
            {
                try
                {
                    var member = m.Module.ResolveMethod(BitConverter.ToInt32(mil, i + 1));
                    if (member?.Name == "get_OwnerIsSecondaryEnemy") sec = true;
                }
                catch { }
                i += 4;
            }
        }
        if (sec)
        {
            Console.WriteLine($"{t.Name}.{m.Name}");
            count++;
        }
    }
}
Console.WriteLine($"Total: {count}");
