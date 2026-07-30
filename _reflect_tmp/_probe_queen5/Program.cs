using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var cm = a.GetType("MegaCrit.Sts2.Core.Combat.CombatManager")!;

foreach (var m in cm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.Name == "EndCombatInternal"))
{
    var attr = m.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "AsyncStateMachineAttribute");
    MethodInfo body = m;
    if (attr != null)
        body = ((Type)attr.ConstructorArguments[0].Value!).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    var il = body.GetMethodBody()!.GetILAsByteArray();
    for (var i = 0; i < il.Length; i++)
    {
        if (il[i] is 0x28 or 0x6F or 0x73)
        {
            try
            {
                var member = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                var n = $"{member!.DeclaringType?.Name}.{member.Name}";
                if (!n.Contains("AsyncTask") && !n.Contains("TaskAwaiter") && !n.Contains("SetResult") && !n.Contains("Await") && !n.Contains("Enumerator") && !n.Contains("MoveNext") && !n.Contains("Dispose") && !n.Contains("ExecutionContext") && !n.Contains("ThrowHelper"))
                    Console.WriteLine(n);
            }
            catch { }
            i += 4;
        }
    }
}
