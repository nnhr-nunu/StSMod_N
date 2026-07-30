using System;
using System.Linq;
using System.Reflection;

var sts2 = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = Assembly.LoadFrom(sts2);

var blood = asm.GetType("MegaCrit.Sts2.Core.Models.Cards.Bloodletting");
Console.WriteLine("Bloodletting methods:");
foreach (var m in blood!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    Console.WriteLine(m.Name);

var wither = asm.GetType("MegaCrit.Sts2.Core.Models.Powers.WitheringPresencePower");
Console.WriteLine("WitheringPresence methods:");
foreach (var m in wither!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    Console.WriteLine(m.Name);

var burnCard = asm.GetType("MegaCrit.Sts2.Core.Models.Cards.Burn");
Console.WriteLine("Burn card methods:");
foreach (var m in burnCard!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    Console.WriteLine(m.Name);

var creature = asm.GetType("MegaCrit.Sts2.Core.Entities.Creatures.Creature");
foreach (var m in creature!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(x => x.Name.Contains("Hp", StringComparison.OrdinalIgnoreCase)))
    Console.WriteLine("Creature." + m.Name + " -> " + m);
