using System.Reflection;

var a = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

void Dump(MethodInfo? m)
{
    if (m?.GetMethodBody() == null)
    {
        Console.WriteLine(m?.DeclaringType?.Name + "." + m?.Name + " no");
        return;
    }
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
    Console.WriteLine("sig: " + m.ReturnType.Name + " (" +
                      string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
    var il = m.GetMethodBody()!.GetILAsByteArray()!;
    var module = m.Module;
    for (var i = 0; i < il.Length; i++)
    {
        var op = il[i];
        switch (op)
        {
            case 0x28 or 0x6F or 0x73:
                try
                {
                    var member = module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    var tag = op == 0x28 ? "call" : op == 0x6F ? "callvirt" : "newobj";
                    Console.WriteLine($"{tag} {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { /* ignore */ }
                i += 4;
                break;
            case 0x72:
                try { Console.WriteLine("ldstr " + module.ResolveString(BitConverter.ToInt32(il, i + 1))); }
                catch { /* ignore */ }
                i += 4;
                break;
            case >= 0x16 and <= 0x1E:
                Console.WriteLine("ldc.i4." + (op - 0x16));
                break;
            case 0x1F:
                Console.WriteLine("ldc.i4.s " + unchecked((sbyte)il[i + 1]));
                i++;
                break;
        }
    }
}

foreach (var typeName in new[]
         {
             "CalculatedVar", "CalculatedDamageVar", "CalculationBaseVar", "ExtraDamageVar"
         })
{
    var t = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars." + typeName)!;
    Console.WriteLine("\n#### " + typeName + " ctors ####");
    foreach (var c in t.GetConstructors())
        Console.WriteLine("ctor(" + string.Join(", ", c.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name)) + ")");
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        Dump(m);
}

// MindBlast Calculate via WithMultiplier - dump get_CanonicalVars fully already known
// Look at CalculatedVar.Calculate
var cv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedVar")!;
Dump(cv.GetMethod("Calculate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!);
Dump(cv.GetMethod("WithMultiplier", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!);

// BodySlam OnPlay d__ MoveNext for damage amount source
foreach (var t in a.GetTypes().Where(t => t.FullName?.Contains("BodySlam") == true && t.Name.Contains("OnPlay")))
{
    var m = t.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Dump(m);
}
foreach (var t in a.GetTypes().Where(t => t.FullName?.Contains("MindBlast") == true && t.Name.Contains("OnPlay")))
{
    var m = t.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Dump(m);
}
