using System;
using System.Reflection;

var sts = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

var m = sts.GetType("MegaCrit.Sts2.Core.Models.CardModel")!.GetMethod("SetToFreeThisTurn")!;
Console.WriteLine(m.IsPublic ? "public" : "non-public");
Console.WriteLine(m.ReturnType.Name);
