using System;
using System.Linq;
using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

// GeneticAlgorithm tags
var ga = a.GetType("MegaCrit.Sts2.Core.Models.Cards.GeneticAlgorithm")!;
DumpTags(ga, "GeneticAlgorithm");
DumpTags(a.GetType("MegaCrit.Sts2.Core.Models.Cards.DefendIronclad")!, "DefendIronclad");
DumpTags(a.GetType("MegaCrit.Sts2.Core.Models.Cards.UltimateDefend")!, "UltimateDefend");

void DumpTags(Type t, string label)
{
    Console.WriteLine("\n=== " + label + " ===");
    var p = t.GetProperty("CanonicalTags", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly)
         ?? t.GetProperties(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public).FirstOrDefault(x => x.Name == "CanonicalTags");
    if (p == null) { Console.WriteLine("no CanonicalTags"); return; }
    // try invoke on uninitialized - may fail; dump IL instead
    var m = p.GetGetMethod(true)!;
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
}

// Who checks CardTag.Defend?
Console.WriteLine("\n=== references to CardTag.Defend / Contains Defend ===");
int count = 0;
foreach (var t in a.GetTypes())
{
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
    {
        MethodInfo move = m;
        try {
            var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
            if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
        } catch { continue; }
        byte[]? il;
        try { il = move.GetMethodBody()?.GetILAsByteArray(); } catch { continue; }
        if (il == null) continue;
        // look for ldc.i4.2 near Contains/Tags - heuristic: string "Defend" or call with CardTag
        for (var i = 0; i < il.Length - 5; i++)
        {
            if (il[i] == 0x6F || il[i] == 0x28) // callvirt/call
            {
                try {
                    var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i+1));
                    if (member?.Name == "Contains" && i >= 2 && il[i-5] == 0x1A) // ldc.i4.2 just before? rough
                    {
                        // check nearby for get_Tags
                    }
                    if (member?.Name == "get_Tags" || (member?.DeclaringType?.Name == "CardTag"))
                    {
                        // scan method for ldc.i4.2 and Contains
                    }
                } catch {}
            }
        }
        // simpler: look for get_Tags in method and Fasten-like pattern
        bool hasTags = false, hasContains = false, hasLdc2 = false;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] == 0x1A) hasLdc2 = true; // ldc.i4.2 = Defend
            if (il[i] is 0x28 or 0x6F)
            {
                try {
                    var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i+1));
                    if (member?.Name == "get_Tags") hasTags = true;
                    if (member?.Name == "Contains") hasContains = true;
                } catch {}
                i += 4;
            }
        }
        if (hasTags && hasContains && hasLdc2)
        {
            Console.WriteLine(t.Name + "." + m.Name);
            count++;
            if (count > 40) { Console.WriteLine("..."); return; }
        }
    }
}
