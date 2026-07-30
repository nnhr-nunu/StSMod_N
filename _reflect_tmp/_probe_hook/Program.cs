using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;

foreach (var m in hook.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "BeforeAttack"))
{
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo move = m;
    if (attr != null)
        move = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = move.GetMethodBody()?.GetILAsByteArray()!;
    Console.WriteLine("Hook.BeforeAttack:");
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = move.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                var name = $"{member!.DeclaringType?.Name}.{member.Name}";
                if (name.Contains("AsyncTask") || name.Contains("TaskAwaiter") || name.Contains("SetResult") ||
                    name.Contains("Enumerator") || name.Contains("MoveNext") || name.Contains("Dispose") ||
                    name.Contains("Await") || name.Contains("ExecutionContext"))
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
