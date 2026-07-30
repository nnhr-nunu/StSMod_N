using System;
using System.Reflection;
var a=Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
foreach (var tn in new[]{"MegaCrit.Sts2.Core.Hooks.ModifyDamageHookType","MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode"})
{
  var t=a.GetType(tn)!;
  foreach (var name in Enum.GetNames(t))
    Console.WriteLine(tn.Split('.').Last()+"."+name+"="+(int)Enum.Parse(t,name));
}
