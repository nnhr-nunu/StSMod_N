using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = a.GetType("MegaCrit.Sts2.Core.Models.Cards.Shiv")!;
var inst = Activator.CreateInstance(t)!;
var tags = (IEnumerable<object>)t.GetMethod("get_CanonicalTags", BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(inst, null)!;
Console.WriteLine("Tags=" + string.Join(",", tags));
