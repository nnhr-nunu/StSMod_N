using System.Reflection;
var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var art = a.GetType("MegaCrit.Sts2.Core.Models.Powers.ArtifactPower")!;
foreach (var name in new[]{"get_ShouldScaleInMultiplayer","get_StackType","get_Type","TryModifyPowerAmountReceived","GetScaledAmountForMultiplayer"})
{
  var m = art.GetMethod(name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);
  if (m == null) { Console.WriteLine(name + " missing"); continue; }
  var il = m.GetMethodBody()?.GetILAsByteArray();
  Console.WriteLine(name + " IL=" + (il == null ? "null" : BitConverter.ToString(il)));
}

// Find Panacea or cards applying Artifact
foreach (var t in a.GetTypes().Where(t => t.Namespace?.Contains("Cards") == true && t.Name.Contains("Panacea") || t.Name == "Ancient"))
  Console.WriteLine(t.FullName);

foreach (var t in a.GetTypes().Where(t => t.IsClass && t.Namespace?.Contains("Models.Cards") == true))
{
  var props = t.GetProperties(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
  // skip - search CanonicalVars for Artifact in IL
}
int n=0;
var artifactType = a.GetType("MegaCrit.Sts2.Core.Models.Powers.ArtifactPower")!;
foreach (var t in a.GetTypes().Where(t => t.Namespace == "MegaCrit.Sts2.Core.Models.Cards"))
{
  byte[]? il = null;
  try {
    var m = t.GetMethod("get_CanonicalVars", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
         ?? t.GetProperty("CanonicalVars", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)?.GetMethod;
  } catch {}
}
// simpler string search in methods for ArtifactPower token
foreach (var t in a.GetTypes().Where(t => t.Namespace == "MegaCrit.Sts2.Core.Models.Cards"))
{
  foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
  {
    var body = m.GetMethodBody()?.GetILAsByteArray();
    if (body == null) continue;
    for (var i = 0; i < body.Length-4; i++)
    {
      if (body[i] != 0xD0) continue; // ldtoken
      try {
        if (m.Module.ResolveType(BitConverter.ToInt32(body, i+1)) == artifactType)
        {
          Console.WriteLine("USES " + t.Name + "." + m.Name);
          n++;
        }
      } catch {}
    }
  }
  if (n > 15) break;
}
