using System.Reflection;
using System.Reflection.Emit;

var a = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

var frail = a.GetType("MegaCrit.Sts2.Core.Models.Powers.FrailPower")!;
var m = frail.GetMethod("ModifyBlockMultiplicative",
    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
Console.WriteLine("ModifyBlockMultiplicative: " + m);
DumpIl(m);

// Compare Weak/Vulnerable for context
foreach (var pair in new[]
         {
             ("WeakPower", "ModifyDamageMultiplicative"),
             ("VulnerablePower", "ModifyDamageMultiplicative"),
         })
{
    var ty = a.GetType("MegaCrit.Sts2.Core.Models.Powers." + pair.Item1)!;
    var mm = ty.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .First(x => x.Name == pair.Item2 && x.DeclaringType == ty);
    Console.WriteLine("=== " + pair.Item1 + "." + pair.Item2 + " ===");
    DumpIl(mm);
}

static void DumpIl(MethodInfo method)
{
    var body = method.GetMethodBody();
    if (body == null)
    {
        Console.WriteLine("  (no body)");
        return;
    }

    var il = body.GetILAsByteArray()!;
    var module = method.Module;
    for (var i = 0; i < il.Length; i++)
    {
        var op = il[i];
        if (op is 0x28 or 0x6F or 0x73) // call / callvirt / newobj
        {
            var token = BitConverter.ToInt32(il, i + 1);
            try
            {
                var member = module.ResolveMethod(token);
                Console.WriteLine($"  {(op == 0x28 ? "call" : op == 0x6F ? "callvirt" : "newobj")} {member!.DeclaringType?.Name}.{member.Name}");
            }
            catch { /* ignore */ }

            i += 4;
        }
        else if (op == 0x72) // ldstr
        {
            var token = BitConverter.ToInt32(il, i + 1);
            try { Console.WriteLine("  ldstr \"" + module.ResolveString(token) + "\""); }
            catch { /* ignore */ }
            i += 4;
        }
        else if (op == 0x6A) // conv.r8 - skip
        {
        }
        else if (op is >= 0x16 and <= 0x1E) // ldc.i4.0 .. ldc.i4.8
        {
            Console.WriteLine("  ldc.i4." + (op - 0x16));
        }
        else if (op == 0x20) // ldc.i4
        {
            Console.WriteLine("  ldc.i4 " + BitConverter.ToInt32(il, i + 1));
            i += 4;
        }
        else if (op == 0x22) // ldc.r4
        {
            Console.WriteLine("  ldc.r4 " + BitConverter.ToSingle(il, i + 1));
            i += 4;
        }
        else if (op == 0x23) // ldc.r8
        {
            Console.WriteLine("  ldc.r8 " + BitConverter.ToDouble(il, i + 1));
            i += 8;
        }
        else if (op == 0x2B || op == 0x2C || op == 0x2D) // br / brfalse / brtrue short
        {
            Console.WriteLine("  branch op=" + op.ToString("X2"));
            i += 1;
        }
    }
}
