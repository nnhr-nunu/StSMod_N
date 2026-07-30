using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
// Confirm UltimateDefend BlockVar props
var t = a.GetType("MegaCrit.Sts2.Core.Models.Cards.UltimateDefend")!;
var m = t.GetProperty("CanonicalVars", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)!.GetGetMethod(true)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
for (var i = 0; i < il.Length; i++)
{
    var op = il[i];
    if (op is 0x28 or 0x6F or 0x73)
    {
        try {
            var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i+1));
            Console.WriteLine((op==0x28?"call":op==0x6F?"callvirt":"newobj") + " " + member!.DeclaringType?.Name + "." + member.Name);
        } catch {}
        i += 4;
    }
    else if (op >= 0x16 && op <= 0x1E) Console.WriteLine($"ldc.i4.{op-0x16}");
    else if (op == 0x1F) { Console.WriteLine($"ldc.i4.s {(sbyte)il[i+1]}"); i++; }
}
// IsPowered: return Move && !Unpowered
var ip = a.GetType("MegaCrit.Sts2.Core.ValueProps.ValuePropExtensions")!.GetMethod("IsPoweredCardOrMonsterMoveBlock")!;
Console.WriteLine("\nIL bytes: " + BitConverter.ToString(ip.GetMethodBody()!.GetILAsByteArray()!));
