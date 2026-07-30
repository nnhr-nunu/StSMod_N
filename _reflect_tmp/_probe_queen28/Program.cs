using System;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;
var m = cm.GetMethod("get_IsEnding", BindingFlags.Public | BindingFlags.Instance)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
for (var i = 0; i < il.Length - 4; i++)
{
    if (il[i] == 0x73) // newobj
    {
        var token = BitConverter.ToInt32(il, i + 1);
        try
        {
            var member = m.Module.ResolveMember(token);
            Console.WriteLine($"newobj token {token:X} -> {member}");
        }
        catch (Exception ex) { Console.WriteLine($"token {token:X} err {ex.Message}"); }
    }
    if (il[i] == 0x7E || il[i] == 0x6F || il[i] == 0x28)
    {
        try
        {
            var member = m.Module.ResolveMember(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"{il[i]:X2} -> {member}");
        }
        catch { }
        i += 4;
    }
}
