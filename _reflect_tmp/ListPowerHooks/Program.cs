using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
// Find CardModel subclasses / ModelDb cards
var cardBase = a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!;
var cards = a.GetTypes().Where(t => t.IsSubclassOf(cardBase) && !t.IsAbstract).OrderBy(t => t.Name).Select(t => t.Name).ToList();
Console.WriteLine("count="+cards.Count);
// Search candidates by english-ish names matching JP list
var hints = new[]{"Dismantle","Brain","Bloodlet","Feed","Carve","Ravage","Brand","Deadly","Skewer","Strangle","Noxious","Kill","Adrenaline","Bury","Door","Soul","Murder","Hang","Revive","Blind","Sever","Compress","Drill","Crush","Shackle","Lacerat","Stomp","Knife","Pummel","Cleave","Whirlwind","Hemokinesis","Offering","Corpse","Terror","Wraith","Gash","Impale","Reaper","Catalyst"};
foreach (var h in hints) {
  var hits = cards.Where(n => n.Contains(h, StringComparison.OrdinalIgnoreCase)).ToList();
  if (hits.Count>0) Console.WriteLine(h+": "+string.Join(", ", hits));
}
// Also dump all card names containing common silent/defect/ironclad patterns - write full list
System.IO.File.WriteAllLines(@"D:\Dev\antigravity\StSMod_N\_all_card_typenames.txt", cards);
Console.WriteLine("wrote all");
