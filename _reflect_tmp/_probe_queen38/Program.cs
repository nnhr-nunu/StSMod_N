using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var torch = a.GetType("MegaCrit.Sts2.Core.Models.Monsters.TorchHeadAmalgam")!;
var method = torch.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    .First(x => x.Name == "AfterAddedToRoom");
var attr = method.GetCustomAttributesData().First(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
var body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
var il = body.GetMethodBody()!.GetILAsByteArray()!;
for (var i = 0; i < il.Length; i++)
{
    if (il[i] == 0x73)
    {
        try
        {
            var member = body.Module.ResolveMember(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"NEW {member}");
        }
        catch (Exception ex) { Console.WriteLine($"NEW err {ex.Message}"); }
        i += 4;
    }
    if (il[i] == 0x28 && il[i+1] == 0x88) // callvirt Apply generic?
    {
        try
        {
            var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"CALL {member}");
        }
        catch { }
        i += 4;
    }
}

var ap = a.GetType("MegaCrit.Sts2.Core.Models.Powers.AdaptablePower")!;
var ir = ap.GetMethod("get_IsReviving", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
il = ir.GetMethodBody()!.GetILAsByteArray()!;
Console.WriteLine("\nIsReviving:");
for (var i = 0; i < il.Length; i++)
{
    if (il[i] is 0x28 or 0x6F)
    {
        try
        {
            var member = ir.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
            Console.WriteLine($"  {member!.DeclaringType?.Name}.{member.Name}");
        }
        catch { }
        i += 4;
    }
}
