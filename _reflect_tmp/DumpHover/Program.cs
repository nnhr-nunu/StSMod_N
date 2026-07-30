using System;
using System.Linq;
using System.Reflection;
var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
foreach (var x in asm.GetTypes().Where(x=>x.BaseType?.Name=="TemporaryStrengthPower" || x.BaseType?.Name=="TemporaryDexterityPower").OrderBy(x=>x.Name))
  Console.WriteLine(x.FullName + " : " + x.BaseType!.Name);
