using System;
using System.Linq;
using System.Reflection;

class P
{
    static void DumpCalls(MethodInfo? m)
    {
        if (m?.GetMethodBody() == null) return;
        Console.WriteLine("=== " + m.DeclaringType!.Name + "." + m.Name + " ===");
        var il = m.GetMethodBody()!.GetILAsByteArray()!;
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
                    Console.WriteLine($"  {tag} {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { /* ignore */ }
                i += 4;
            }
            else if (op == 0x72)
            {
                try { Console.WriteLine("  ldstr \"" + module.ResolveString(BitConverter.ToInt32(il, i + 1)) + "\""); }
                catch { /* ignore */ }
                i += 4;
            }
        }
    }

    static bool UsesCalculated(Type t)
    {
        var p = t.GetProperty("CanonicalVars",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        var g = p?.GetGetMethod(true);
        if (g?.GetMethodBody() == null) return false;
        var il = g.GetMethodBody()!.GetILAsByteArray()!;
        var mod = g.Module;
        for (var i = 0; i < il.Length - 4; i++)
        {
            if (il[i] != 0x73) continue;
            try
            {
                var ctor = mod.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                if (ctor.DeclaringType?.Name is "CalculatedVar" or "CalculatedDamageVar" or "CalculatedBlockVar")
                    return true;
            }
            catch { /* ignore */ }
        }
        return false;
    }

    static void Main()
    {
        var a = Assembly.LoadFrom(
            @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

        // DynamicVar string formatting
        var dv = a.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar")!;
        DumpCalls(dv.GetMethod("ToHighlightedString",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        DumpCalls(dv.GetMethod("ToString",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null));

        // find Loc helpers mentioning diff / hide
        foreach (var t in a.GetTypes())
        {
            if ((t.FullName ?? "").Contains("Loc", StringComparison.Ordinal) == false
                && (t.Name.Contains("Dynamic", StringComparison.Ordinal) == false))
                continue;
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                           BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var body = m.GetMethodBody();
                if (body == null) continue;
                var il = body.GetILAsByteArray()!;
                var mod = m.Module;
                for (var i = 0; i < il.Length - 4; i++)
                {
                    if (il[i] != 0x72) continue;
                    try
                    {
                        var s = mod.ResolveString(BitConverter.ToInt32(il, i + 1));
                        if (s.Contains("diff", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("hide", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("combat", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("Mind Blast", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("draw pile", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("Ethereal", StringComparison.OrdinalIgnoreCase))
                            Console.WriteLine($"{t.Name}.{m.Name}: \"{s}\"");
                    }
                    catch { /* ignore */ }
                }
            }
        }

        // Necrobinder calculated cards - dump CanonicalVars construction + static calcs
        var necroPool = a.GetType("MegaCrit.Sts2.Core.Models.CardPools.NecrobinderCardPool");
        Console.WriteLine("Necro pool: " + necroPool);
        foreach (var name in new[]
                 {
                     "SoulStorm", "MementoMori", "DeathMarch", "PullFromBelow", "Murder", "Rattle", "Rend",
                     "AshenStrike", "TimesUp", "NoEscape"
                 })
        {
            var t = a.GetType("MegaCrit.Sts2.Core.Models.Cards." + name);
            if (t == null) continue;
            Console.WriteLine("\n#### " + name + " calc=" + UsesCalculated(t));
            var canon = t.GetProperty("CanonicalVars",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            DumpCalls(canon?.GetGetMethod(true));
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                           BindingFlags.Instance | BindingFlags.DeclaredOnly))
                if (m.IsStatic || m.Name.Contains("Calc"))
                    DumpCalls(m);
        }
    }
}
