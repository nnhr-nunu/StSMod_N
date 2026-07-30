using System; using System.Linq; using System.Reflection;
var a = Assembly.LoadFrom(args[0]);
var t = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook");
foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m => m.Name.Contains("AfterSideTurnEnd")))
  Console.WriteLine(m);
