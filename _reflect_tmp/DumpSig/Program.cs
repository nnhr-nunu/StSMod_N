using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

foreach (var m in typeof(CardModel).GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
  if (m.Name.Contains("Enqueue") || m.Name.Contains("PlayVfx") || m.Name == "OnPlay")
    Console.WriteLine(m);

foreach (var m in typeof(CreatureCmd).GetMethods(BindingFlags.Static|BindingFlags.Public))
  if (m.Name.Contains("Trigger") || m.Name.Contains("Anim"))
    Console.WriteLine(m);

var inflame = typeof(Inflame);
foreach (var m in inflame.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
  if (m.Name.Contains("Enqueue") || m.Name.Contains("Play"))
    Console.WriteLine("Inflame: " + m);
