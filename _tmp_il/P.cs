using System;
using System.Linq;
using System.Reflection;

class StrikeDummyDump
{
    static void Main()
    {
        var a = Assembly.LoadFrom(
            @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
        var t = a.GetType("MegaCrit.Sts2.Core.Models.Relics.StrikeDummy")!;
        var m = t.GetMethod("ModifyDamageAdditive", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
        Dump(m);

        var cmd = a.GetType("MegaCrit.Sts2.Core.Commands.CreatureCmd")!;
        foreach (var dm in cmd.GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .Where(x => x.Name == "Damage"))
            Console.WriteLine("CreatureCmd.Damage: " + string.Join(", ", dm.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)));
    }

    static void Dump(MethodInfo m)
    {
        Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
        for (var i = 0; i < il.Length;)
        {
            var op = il[i];
            var line = $"[{i:X3}] ";
            switch (op)
            {
                case 0x02: line += "ldarg.0"; i++; break;
                case 0x03: line += "ldarg.1"; i++; break;
                case 0x04: line += "ldarg.2"; i++; break;
                case 0x05: line += "ldarg.3"; i++; break;
                case 0x06: line += "ldarg.4"; i++; break;
                case 0x07: line += "ldarg.5"; i++; break;
                case 0x16: line += "ldc.i4.0"; i++; break;
                case 0x17: line += "ldc.i4.1"; i++; break;
                case 0x2A: line += "ret"; i++; break;
                case 0x2B: line += $"br.s -> {i + 2 + (sbyte)il[i + 1]:X3}"; i += 2; break;
                case 0x2C: line += $"brfalse.s -> {i + 2 + (sbyte)il[i + 1]:X3}"; i += 2; break;
                case 0x2D: line += $"brtrue.s -> {i + 2 + (sbyte)il[i + 1]:X3}"; i += 2; break;
                case 0x28:
                case 0x6F:
                {
                    var mem = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1))!;
                    line += (op == 0x28 ? "call " : "callvirt ") + mem.DeclaringType!.Name + "." + mem.Name;
                    i += 5;
                    break;
                }
                case 0x8C:
                {
                    var type = m.Module.ResolveType(BitConverter.ToInt32(il, i + 1))!;
                    line += "box " + type.Name;
                    i += 5;
                    break;
                }
                default: line += $"op_{op:X2}"; i++; break;
            }

            Console.WriteLine(line);
        }
    }
}
