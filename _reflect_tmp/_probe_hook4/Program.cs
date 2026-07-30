using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");
var hook = a.GetType("MegaCrit.Sts2.Core.Hooks.Hook")!;
var m = hook.GetMethods(BindingFlags.Public | BindingFlags.Static).First(x => x.Name == "ModifyUnblockedDamageTarget");
var il = m.GetMethodBody()!.GetILAsByteArray();
for (var i = 0; i < il.Length; i++)
{
    var op = il[i];
  string line = $"{i,4}: {op:X2}";
  if (op is 0x28 or 0x6F or 0x73)
  {
    var member = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
    line += $" -> {member!.DeclaringType?.Name}.{member.Name}";
    i += 4;
  }
  else if (op == 0x1F) { line += $" {(sbyte)il[i+1]:X2}"; i++; }
  else if (op >= 0x16 && op <= 0x1E) line += $" ({op-0x16})";
  Console.WriteLine(line);
}
