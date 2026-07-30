using System.Reflection;

var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var card = a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!;
var m = card.GetMethod("get_IsRemovable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
for (int i = 0; i < il.Length; i++)
{
    byte op = il[i];
    if (op is >= 0x16 and <= 0x1E) Console.WriteLine($"ldc.i4.{op - 0x16}");
    else if (op == 0x1F) { Console.WriteLine($"ldc.i4.s {(sbyte)il[i+1]}"); i++; }
    else if (op == 0x20) { Console.WriteLine($"ldc.i4 {BitConverter.ToInt32(il, i+1)}"); i += 4; }
    else if (op is 0x28 or 0x6F)
    {
        var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
        Console.WriteLine($"{(op==0x28?"call":"callvirt")} {member.DeclaringType?.Name}.{member.Name}");
        i += 4;
    }
}
