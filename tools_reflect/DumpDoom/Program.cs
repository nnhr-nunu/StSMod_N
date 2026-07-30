using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
void DumpAsync(Type type, string methodName)
{
    Console.WriteLine($"\n===== {type.Name}.{methodName} =====");
    foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(x => x.Name == methodName))
    {
        var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
        MethodInfo move = m;
        if (attr != null) move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
        var body = move.GetMethodBody();
        if (body == null) continue;
        var il = body.GetILAsByteArray()!;
        for (var i = 0; i < il.Length; i++)
        {
            var op = il[i];
            if (op is 0x28 or 0x6F or 0x73)
            {
                var token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    var member = move.Module.ResolveMethod(token);
                    var name = $"{member!.DeclaringType?.Name}.{member.Name}";
                    if (name.Contains("AsyncTaskMethodBuilder") || name.Contains("TaskAwaiter") || name.Contains("get_IsCompleted") || name.Contains("GetResult") || name.Contains("SetResult") || name.Contains("SetException") || name.Contains("SetStateMachine") || name.Contains("get_Task") || name.Contains("AwaitUnsafe") || name.Contains("AwaitOnCompleted") || name.Contains("ExecutionContext") || name.Contains("ExceptionDispatchInfo"))
                    { i += 4; continue; }
                    Console.WriteLine($"  {(op == 0x28 ? "call" : op == 0x6F ? "callvirt" : "newobj")} {member.DeclaringType?.FullName}.{member.Name}");
                }
                catch { }
                i += 4;
            }
        }
    }
}

var cmd = a.GetType("MegaCrit.Sts2.Core.Commands.CreatureCmd")!;
DumpAsync(cmd, "KillWithoutCheckingWinCondition");
