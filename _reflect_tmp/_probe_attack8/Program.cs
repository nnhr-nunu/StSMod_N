using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
var m = ac.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "Execute");
var attr = m.GetCustomAttributesData().First(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
var move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
var il = move.GetMethodBody()!.GetILAsByteArray();

// find List Count checks
for (var i = 0; i < il.Length - 6; i++)
{
    if (il[i] == 0x6F && il[i+5] == 0x2A) // call ... ret pattern nearby
    {
        try
        {
            var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            if (member?.Name == "get_Count")
            {
                Console.WriteLine($"get_Count at {i}, next bytes: {il[i+5]:X2} {il[i+6]:X2} {il[i+7]:X2}");
            }
        }
        catch {}
    }
}

// search for brfalse after get_Count
for (var i = 0; i < il.Length - 10; i++)
{
    if (il[i] == 0x6F)
    {
        try
        {
            var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            if (member?.Name == "get_Count")
            {
                Console.WriteLine($"Count check region {i}:");
                for (var j = i; j < Math.Min(i+20, il.Length); j++)
                    Console.Write($"{il[j]:X2} ");
                Console.WriteLine();
            }
        }
        catch {}
        i += 4;
    }
}
