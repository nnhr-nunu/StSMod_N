using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
using var fs = File.OpenRead(path);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();
foreach (var th in md.TypeDefinitions)
{
  var t = md.GetTypeDefinition(th);
  if (md.GetString(t.Name) != "NGameOverScreen") continue;
  Console.WriteLine(md.GetString(t.Namespace));
}
