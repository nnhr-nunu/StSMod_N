using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

void DumpAsync(Type type, string methodName)
{
    Console.WriteLine($"\n===== {type.Name}.{methodName} =====");
    foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(x => x.Name == methodName))
    {
        var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
        MethodInfo move = m;
        if (attr != null)
            move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
        var il = move.GetMethodBody()?.GetILAsByteArray();
        if (il == null) continue;
        for (var i = 0; i < il.Length; i++)
        {
            if (il[i] is 0x28 or 0x6F or 0x73)
            {
                try
                {
                    var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    var name = $"{member!.DeclaringType?.Name}.{member.Name}";
                    if (name.Contains("AsyncTask") || name.Contains("TaskAwaiter") || name.Contains("SetResult") ||
                        name.Contains("SetException") || name.Contains("SetStateMachine") || name.Contains("get_Task") ||
                        name.Contains("Await") || name.Contains("ExecutionContext") || name.Contains("ExceptionDispatch") ||
                        name.Contains("Enumerator") || name.Contains("MoveNext") || name.Contains("Dispose"))
                    {
                        i += 4; continue;
                    }
                    Console.WriteLine($"  {member.DeclaringType?.Name}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

var ac = a.GetType("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand")!;
DumpAsync(ac, "Execute");
DumpAsync(ac, "GetTargets");

// properties TargetSide, Attacker
foreach (var p in ac.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    Console.WriteLine($"prop {p.Name} : {p.PropertyType.Name}");
