using System;
using System.Linq;
using System.Reflection;

var sts = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

foreach (var t in sts.GetTypes().Where(t => t.Name.Contains("OnPlayWrapper")))
    Console.WriteLine(t.FullName);
