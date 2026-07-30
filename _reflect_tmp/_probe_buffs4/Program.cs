using System;
using System.Linq;
using System.Reflection;

var a = Assembly.LoadFrom(@"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpFrail\bin\Release\net9.0\sts2.dll");

foreach (var name in new[] { "MinionPower", "ArtifactPower", "TerritorialPower", "SoarPower", "BufferPower", "IntangiblePower", "HardToKillPower", "HardenedShellPower", "IllusionPower" })
{
    var t = a.GetType($"MegaCrit.Sts2.Core.Models.Powers.{name}")!;
    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        if (m.Name.Contains("RemovedOnDeath") || m.Name.Contains("ShouldPowerBeRemoved"))
        {
            var il = m.GetMethodBody()?.GetILAsByteArray();
            Console.WriteLine($"{name}.{m.Name} IL={BitConverter.ToString(il ?? [])}");
        }
    }
}
