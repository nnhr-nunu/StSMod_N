using System;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AsleepPower")!;
var typeProp = ap.GetProperty("Type", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
var stackProp = ap.GetProperty("StackType", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
Console.WriteLine($"AsleepPower.Type getter on class: {typeProp?.GetGetMethod(nonPublic: true)?.IsPublic}");
var m = ap.GetMethod("get_Type", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
var il = m.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("AsleepPower.get_Type IL: " + BitConverter.ToString(il));
