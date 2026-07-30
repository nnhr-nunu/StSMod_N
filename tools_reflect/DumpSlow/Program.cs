using System.Reflection;

var a = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

void DumpCalls(MethodInfo m)
{
    var il = m.GetMethodBody()?.GetILAsByteArray();
    if (il == null) { Console.WriteLine(m.Name + " no il"); return; }
    Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
    var module = m.Module;
    for (var i = 0; i < il.Length; i++)
    {
        var op = il[i];
        if (op is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                var tag = op == 0x28 ? "call" : op == 0x6F ? "callvirt" : "newobj";
                Console.WriteLine($"{tag} {member.DeclaringType?.Name}.{member.Name}");
            }
            catch { /* ignore */ }
            i += 4;
        }
        else if (op == 0x72)
        {
            try { Console.WriteLine("ldstr " + module.ResolveString(BitConverter.ToInt32(il, i + 1))); }
            catch { /* ignore */ }
            i += 4;
        }
        else if (op == 0xD0)
        {
            try { Console.WriteLine("ldtoken " + module.ResolveType(BitConverter.ToInt32(il, i + 1)).Name); }
            catch { /* ignore */ }
            i += 4;
        }
    }
}

foreach (var t in a.GetTypes().Where(t => t.Name.Contains("SlashMove") || t.Name.Contains("WakeMove") || t.FullName?.Contains("BygoneEffigy") == true))
{
    if (t.Name.Contains("d__"))
    {
        var m = t.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (m != null) DumpCalls(m);
    }
}

// Any ldtoken SlowPower in whole assembly (sample)
var slow = a.GetType("MegaCrit.Sts2.Core.Models.Powers.SlowPower")!;
var n = 0;
foreach (var t in a.GetTypes())
{
    MethodInfo[] methods;
    try { methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly); }
    catch { continue; }
    foreach (var m in methods)
    {
        byte[]? il;
        try { il = m.GetMethodBody()?.GetILAsByteArray(); } catch { continue; }
        if (il == null) continue;
        for (var i = 0; i < il.Length - 4; i++)
        {
            if (il[i] != 0xD0) continue;
            try
            {
                if (t.Module.ResolveType(BitConverter.ToInt32(il, i + 1)) == slow)
                {
                    Console.WriteLine("APPLY " + t.FullName + "." + m.Name);
                    n++;
                }
            }
            catch { /* ignore */ }
        }
    }
    if (n > 20) break;
}
Console.WriteLine("total slow tokens listed above");
