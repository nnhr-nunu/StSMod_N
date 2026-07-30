using System.Reflection;

var asm = Assembly.LoadFrom(@"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.8\lib\net9.0\BaseLib.dll");
foreach (var t in asm.GetTypes().Where(x => x.FullName?.Contains("Config") == true).OrderBy(x => x.FullName))
{
    Console.WriteLine(t.FullName);
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        Console.WriteLine("  M: " + m);
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        Console.WriteLine("  P: " + p.Name + " : " + p.PropertyType.Name);
}
