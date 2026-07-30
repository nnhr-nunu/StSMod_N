using System.Reflection;

var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var vf = a.GetTypes().First(x => x.Name == "VoidFormPower");

void DumpMoveNext(Type nested)
{
    var moveNext = nested.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .FirstOrDefault(m => m.Name == "MoveNext");
    if (moveNext?.GetMethodBody() == null) return;
    Console.WriteLine("=== " + nested.Name + " ===");
    var il = moveNext.GetMethodBody()!.GetILAsByteArray()!;
    for (int i = 0; i < il.Length; i++)
    {
        if (il[i] is not (0x28 or 0x6F)) continue;
        try
        {
            var member = moveNext.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine("  " + member.DeclaringType?.Name + "." + member.Name);
        }
        catch { }
        i += 4;
    }
}

foreach (var nested in vf.GetNestedTypes(BindingFlags.NonPublic))
    if (nested.Name.Contains("d__"))
        DumpMoveNext(nested);

var data = vf.GetNestedTypes(BindingFlags.NonPublic).FirstOrDefault(t => t.Name == "Data");
if (data != null)
    foreach (var f in data.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        Console.WriteLine("Data." + f.Name + " : " + f.FieldType.Name);
