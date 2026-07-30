using System.Reflection;

var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

void DumpType(string name)
{
    var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
    if (t == null) { Console.WriteLine($"MISSING {name}"); return; }
    Console.WriteLine($"\n=== {t.FullName} ===");
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        if (p.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
    foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        if (f.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"  field {f.FieldType.Name} {f.Name}");
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        if (m.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
}

DumpType("CardPlay");
DumpType("CardModel");
DumpType("PlayCardAction");

foreach (var t in a.GetTypes().Where(t => t.Name.Contains("Replay", StringComparison.OrdinalIgnoreCase) && t.Namespace?.Contains("MegaCrit") == true).Take(30))
    Console.WriteLine($"TYPE {t.FullName}");

// cards setting BaseReplayCount in OnPlay
var cardBase = a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!;
foreach (var t in a.GetTypes().Where(t => t.IsSubclassOf(cardBase)))
{
    foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    {
        if (m.Name is not ("OnPlay" or "AfterAutoPrePlayPhaseEnteredEarly" or "AfterAutoPrePlayPhaseEntered" or "AfterAutoPrePlayPhaseEnteredLate"))
            continue;
        var body = m.GetMethodBody();
        if (body == null) continue;
        var il = body.GetILAsByteArray();
        if (il == null) continue;
        for (var i = 0; i < il.Length - 4; i++)
        {
            if (il[i] is 0x7D or 0x6F or 0x28) // stfld, callvirt, call
            {
                try
                {
                    var member = m.Module.ResolveMember(BitConverter.ToInt32(il, i + 1));
                    if (member?.Name?.Contains("BaseReplayCount", StringComparison.OrdinalIgnoreCase) == true)
                        Console.WriteLine($"SETS REPLAY: {t.Name}.{m.Name}");
                }
                catch { }
            }
        }
    }
}
