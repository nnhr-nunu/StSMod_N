using System.Reflection;
var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = asm.GetType("MegaCrit.Sts2.Core.Models.Powers.SlowPower")!;
var m = t.GetMethod("AfterCardPlayed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
var il = m.GetMethodBody()?.GetILAsByteArray();
for (var i = 0; i < il!.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var mem = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"{mem!.DeclaringType!.Name}.{mem.Name}");
        }
        catch { }
        i += 4;
    }
    else if (il[i] == 0x20)
    {
        var val = BitConverter.ToInt32(il, i + 1);
        Console.WriteLine($"ldc.i4 {val}");
        i += 4;
    }
}
