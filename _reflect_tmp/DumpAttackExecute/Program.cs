using System;
using System.Linq;
using System.Reflection;

class P
{
    static void Main()
    {
        var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
        var t = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
        var execute = t.GetMethod("Execute")!;
        Console.WriteLine("Execute return: " + execute.ReturnType.FullName);
        if (execute.ReturnType.IsGenericType)
            Console.WriteLine("  generic arg: " + execute.ReturnType.GetGenericArguments()[0].FullName);

        var results = t.GetProperty("Results")!;
        Console.WriteLine("Results: " + results.PropertyType.FullName);
        var elem = results.PropertyType.GetGenericArguments()[0];
        Console.WriteLine("Results element: " + elem.FullName);
        foreach (var p in elem.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            Console.WriteLine("  " + p.Name + ": " + p.PropertyType.Name);

        var dr = a.GetType("MegaCrit.Sts2.Core.Entities.Creatures.DamageResult")!;
        Console.WriteLine("DamageResult:");
        foreach (var p in dr.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            Console.WriteLine("  " + p.Name + ": " + p.PropertyType.Name);

        // search cards with Heal after Attack
        foreach (var type in a.GetTypes().Where(x => x.Namespace?.Contains("Models.Cards") == true))
        {
            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (m.Name != "OnPlay") continue;
                var body = m.GetMethodBody();
                if (body == null) continue;
                var il = body.GetILAsByteArray()!;
                var module = m.Module;
                var hasAttack = false;
                var hasHeal = false;
                for (int i = 0; i < il.Length - 4; i++)
                {
                    if (il[i] is not (0x28 or 0x6F)) continue;
                    try
                    {
                        var member = module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                        if (member.DeclaringType?.Name == "AttackCommand" && member.Name == "Execute") hasAttack = true;
                        if (member.DeclaringType?.Name == "CreatureCmd" && member.Name == "Heal") hasHeal = true;
                    }
                    catch { }
                }
                if (hasAttack && hasHeal)
                    Console.WriteLine("Card with Attack+Heal: " + type.Name);
            }
        }

        // search for UnblockedDamage usage in cards
        foreach (var type in a.GetTypes().Where(x => x.Namespace?.Contains("Models.Cards") == true))
        {
            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var body = m.GetMethodBody();
                if (body == null) continue;
                var il = body.GetILAsByteArray()!;
                var module = m.Module;
                for (int i = 0; i < il.Length - 4; i++)
                {
                    if (il[i] is not (0x28 or 0x6F)) continue;
                    try
                    {
                        var member = module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                        if (member.Name == "get_UnblockedDamage")
                        {
                            Console.WriteLine(type.Name + "." + m.Name + " uses UnblockedDamage");
                            break;
                        }
                    }
                    catch { }
                }
            }
        }
